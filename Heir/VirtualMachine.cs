using Heir.Syntax;
using Heir.CodeGeneration;
using Heir.Diagnostics;
using Heir.Runtime;
using Heir.Runtime.Intrinsics;
using Heir.Runtime.Values;
using Spectre.Console;

namespace Heir;

internal sealed class ExitMarker;

public sealed class VirtualMachine
{
    public DiagnosticBag Diagnostics { get; }
    public Scope GlobalScope { get; }
    public Scope Scope { get; private set; }
    public Stack<StackFrame> Stack { get; } = [];
    private int _recursionDepth;
    
    private const int _maxRecursionDepth = 20_000;
    private readonly Stack<CallStackFrame> _callStack = [];

    private Bytecode _bytecode;
    private Scope _enclosingScope;
    private int _pointer;

    public VirtualMachine(Bytecode bytecode, DiagnosticBag diagnostics, Scope? scope = null, int recursionDepth = 0)
    {
        Diagnostics = diagnostics;
        GlobalScope = new Scope();
        Scope = scope ?? GlobalScope;
        _enclosingScope = Scope;
        _bytecode = bytecode;
        _recursionDepth = recursionDepth;
    }
    
    public object? Evaluate()
    {
        Intrinsics.RegisterGlobalValues(GlobalScope);
        while (_pointer < _bytecode.Count)
        {
            var instruction = _bytecode[_pointer];
            var result = EvaluateInstruction(instruction);
            if (result?.Value is ExitMarker) break;
        }
        
        return Stack.TryPeek(out var stackFrame)
            ? stackFrame.Value
            : null;
    }

    public void EndRecursion(int level = 1) => _recursionDepth -= level;
    public void BeginRecursion(Token token)
    {
        if (_recursionDepth++ < _maxRecursionDepth) return;
        Diagnostics.RuntimeError(DiagnosticCode.H017, $"Stack overflow: Recursion depth of {_maxRecursionDepth} exceeded", token);
    }
    
    private StackFrame? EvaluateInstruction(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.EXIT:
                return StackFrame.ExitMarker;
            case OpCode.NOOP:
                Advance();
                break;

            case OpCode.BEGINSCOPE:
                _enclosingScope = Scope;
                Scope = new Scope(_enclosingScope);
                Advance();
                break;
            case OpCode.ENDSCOPE:
                Scope = _enclosingScope;
                _enclosingScope = Scope.Enclosing ?? GlobalScope;
                Advance();
                break;

            case OpCode.PROC:
            {
                var bodyBytecode = (List<Instruction>)instruction.Operand!;
                var function = new FunctionValue(bodyBytecode, new Scope(Scope));
                Stack.Push(new(instruction.Root, function));
                Advance();
                break;
            }
            case OpCode.CALL:
            {
                if (instruction.Operand is not ValueTuple<int, List<string>> data)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute CALL op-code: Provided operand is not a tuple containing parameter names & the amount of argument instructions.\nGot: {Markup.Escape(instruction.Operand?.GetType().ToString() ?? "null")}",
                        instruction.Root?.GetFirstToken());

                    break;
                }

                var (argumentInstructionsCount, parameterNames) = data;
                var argumentsBytecode = _bytecode.Skip(_pointer + 1).Take(argumentInstructionsCount);
                var argumentVM = new VirtualMachine(argumentsBytecode, Diagnostics, Scope, _recursionDepth);
                argumentVM.Evaluate();

                var parameterIndex = 0;
                var argumentDefinitionBytecode = argumentVM.Stack
                    .TakeLast(parameterNames.Count)
                    .Reverse()
                    .SelectMany<StackFrame, Instruction>(argumentFrame =>
                    {
                        var parameterName = parameterNames.ElementAtOrDefault(parameterIndex++);
                        return
                        [
                            new(argumentFrame.Node, OpCode.PUSH, parameterName ?? "???"),
                            new(argumentFrame.Node, OpCode.PUSH, argumentFrame.Value),
                            new(argumentFrame.Node, OpCode.STORE, false)
                        ];
                    })
                    .ToList();
                
                var calleeFrame = Stack.Pop();
                if (calleeFrame.Value is not FunctionValue and not IntrinsicFunction)
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute CALL op-code: Loaded callee is not a function, got {Markup.Escape(calleeFrame.Value?.GetType().ToString() ?? "null")}.\nCallee frame node: {calleeFrame.Node}",
                        instruction.Root?.GetFirstToken());
                
                Advance(argumentInstructionsCount);
                if (calleeFrame.Value is FunctionValue function)
                {
                    List<Instruction> bodyBytecode =
                    [
                        new(calleeFrame.Node, OpCode.BEGINSCOPE),
                        ..argumentDefinitionBytecode,
                        ..function.BodyBytecode.Skip(1)
                    ];
                    
                    var isTailCall = _callStack.TryPeek(out var currentState) &&
                                     currentState.EnclosingPointer == _pointer + 1 &&
                                     currentState.Bytecode.Contains(function.BodyBytecode.Skip(1).ToList()) &&
                                     currentState.Closure.Equals(Scope) &&
                                     IsLastOperationBeforeReturn(currentState.Bytecode.Instructions, currentState.EnclosingPointer - 1);
                    
                    if (!isTailCall)
                        _callStack.Push(new(_bytecode, Scope, _pointer + 1));

                    BeginRecursion(instruction.Root?.GetFirstToken() ?? TokenFactory.Identifier("???", Location.Empty, Location.Empty));
                    _bytecode = new Bytecode(bodyBytecode);
                    _pointer = 0;
                    Scope = function.Closure;
                }
                else if (calleeFrame.Value is IntrinsicFunction intrinsicFunction)
                {
                    var argumentValues = argumentVM.Stack
                        .TakeLast(intrinsicFunction.Arity)
                        .Reverse()
                        .Select(frame => frame.Value)
                        .ToList();
                    
                    var result = intrinsicFunction.Invoke(argumentValues);
                    Stack.Push(new(instruction.Root, result));
                    Advance();
                }
                
                break;
            }
            case OpCode.RETURN:
            {
                if (_callStack.TryPop(out var returnState))
                {
                    _bytecode = returnState.Bytecode;
                    _pointer = returnState.EnclosingPointer;
                    Scope = returnState.Closure;
                    EndRecursion();
                }
                else
                    Advance();
                
                break;
            }

            case OpCode.INDEX:
            {
                var indexFrame = Stack.Pop();
                var objectFrame = Stack.Pop();
                
                if (indexFrame.Value is null)
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        "Failed to execute INDEX op-code: Loaded index is null",
                        instruction.Root?.GetFirstToken());
                
                if (objectFrame.Value is not ObjectValue objectValue)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        "Failed to execute INDEX op-code: Loaded object is not an object dictionary",
                        instruction.Root?.GetFirstToken());
                    
                    break;
                }
                
                var value = objectValue[indexFrame.Value];
                Stack.Push(new(objectFrame.Node, value));
                Advance();
                break;
            }

            case OpCode.PUSH:
            case OpCode.PUSHNONE:
                Stack.Push(CreateStackFrameFromInstruction());
                Advance();
                break;
            case OpCode.PUSHOBJECT:
            {
                var bytecodeDictionaryFrame = CreateStackFrameFromInstruction();
                var bytecodeDictionary =
                    (Dictionary<List<Instruction>, List<Instruction>>)bytecodeDictionaryFrame.Value!;
                var evaluatedDictionary = new ObjectValue(
                    bytecodeDictionary
                        .ToList()
                        .ConvertAll(pair =>
                        {
                            var keyVM = new VirtualMachine(new Bytecode(pair.Key), Diagnostics, Scope);
                            var valueVM = new VirtualMachine(new Bytecode(pair.Value), Diagnostics, Scope);
                            var key = keyVM.Evaluate()!;
                            var value = valueVM.Evaluate();
                            return new KeyValuePair<object, object?>(key, value);
                        })
                );

                Stack.Push(new StackFrame(bytecodeDictionaryFrame.Node, evaluatedDictionary));
                Advance();
                break;
            }

            case OpCode.POP:
            {
                Stack.Pop();
                Advance();
                break;
            }
            case OpCode.SWAP:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                Stack.Push(right);
                Stack.Push(left);
                Advance();
                break;
            }
            case OpCode.DUP:
            {
                var value = Stack.Peek();
                Stack.Push(value);
                Advance();
                break;
            }

            case OpCode.LOAD:
            {
                var nameFrame = Stack.Pop();
                if (nameFrame.Value is not string name)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute LOAD op-code: No variable name was located in the stack, got {nameFrame.Value ?? "none"}",
                        nameFrame.Node?.GetFirstToken());
                    
                    break;
                }

                var value = Scope.Lookup(name);
                Stack.Push(new StackFrame(nameFrame.Node, value));
                Advance();
                break;
            }
            case OpCode.STORE:
            {
                var initializerFrame = Stack.Pop();
                var nameFrame = Stack.Pop();
                if (nameFrame.Value is not string name)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute STORE op-code: No variable name was located in the stack, got {nameFrame.Value ?? "none"}",
                        initializerFrame.Node?.GetFirstToken());
                    
                    break;
                }

                if (Scope.IsDeclared(name))
                    Scope.Assign(name, initializerFrame.Value);
                else
                    Scope.Define(name, initializerFrame.Value);

                if (instruction.Operand is true)
                    Stack.Push(initializerFrame);

                Advance();
                break;
            }
            case OpCode.STOREINDEX:
            {
                var initializerFrame = Stack.Pop();
                var indexFrame = Stack.Pop();
                var objectFrame = Stack.Pop();
                if (objectFrame.Value is not ObjectValue objectValue)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute STOREINDEX op-code: No object to index was located in the stack, got {objectFrame.Value ?? "none"}",
                        initializerFrame.Node?.GetFirstToken());
                    
                    break;
                }
                if (indexFrame.Value is null)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute STOREINDEX op-code: Expected frame for object index has null operand",
                        indexFrame.Node?.GetFirstToken());
                    
                    break;
                }

                objectValue[indexFrame.Value!] = initializerFrame.Value;
                if (instruction.Operand is true)
                    Stack.Push(initializerFrame);

                Advance();
                break;
            }

            case OpCode.CONCAT:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToString(left.Value) + Convert.ToString(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            
            case OpCode.ADD:
            case OpCode.SUB:
            case OpCode.MUL:
            case OpCode.DIV:
            case OpCode.IDIV:
            case OpCode.MOD:
            case OpCode.POW:
            case OpCode.LT:
            case OpCode.LTE:
            case OpCode.GT:
            case OpCode.GTE:
            {
                var right = Convert.ToDouble(Stack.Pop().Value);
                var left = Convert.ToDouble(Stack.Pop().Value);
                var calculate = BinaryTypeOperations.Double.GetValueOrDefault(instruction.OpCode);
                if (calculate == null)
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Unhandled binary double operation for op-code {instruction.OpCode}",
                        instruction.Root?.GetFirstToken());

                var result = calculate(left, right);
                Stack.Push(new StackFrame(instruction.Root, result));
                Advance();
                break;
            }
            case OpCode.BAND:
            case OpCode.BOR:
            case OpCode.BXOR:
            {
                var right = Convert.ToInt64(Stack.Pop().Value);
                var left = Convert.ToInt64(Stack.Pop().Value);
                var calculate = BinaryTypeOperations.Long.GetValueOrDefault(instruction.OpCode);
                if (calculate == null)
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Unhandled binary long operation for op-code {instruction.OpCode}",
                        instruction.Root?.GetFirstToken());

                var result = calculate(left, right);
                Stack.Push(new StackFrame(instruction.Root, result));
                Advance();
                break;
            }
            case OpCode.BSHL:
            case OpCode.BSHR:
            {
                var right = Convert.ToInt32(Stack.Pop().Value);
                var left = Convert.ToInt32(Stack.Pop().Value);
                var calculate = BinaryTypeOperations.Int.GetValueOrDefault(instruction.OpCode);
                if (calculate == null)
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Unhandled binary int operation for op-code {instruction.OpCode}",
                        instruction.Root?.GetFirstToken());

                var result = calculate(left, right);
                Stack.Push(new StackFrame(instruction.Root, result));
                Advance();
                break;
            }
            case OpCode.UNM:
            {
                var operand = Stack.Pop();
                if (operand.Value is int i)
                    Stack.Push(new StackFrame(operand.Node, -i));
                else
                    Stack.Push(new StackFrame(operand.Node, -Convert.ToDouble(operand.Value)));
                
                Advance();
                break;
            }
            
            case OpCode.DEC:
            case OpCode.INC:
            {
                if (instruction.Operand is not string name)
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute {instruction.OpCode} op-code: Operand is not a string",
                        instruction.Root?.GetFirstToken());

                    break;
                }
                if (!Scope.IsDefined(name))
                {
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Failed to execute {instruction.OpCode} op-code: Operand provided '{name}' (identifier name to increment/decrement) was not found in the scope",
                        instruction.Root?.GetFirstToken());

                    break;
                }
                
                var previousValue = Convert.ToDouble(Scope.Lookup(name)!);
                var value = previousValue + (instruction.OpCode == OpCode.INC ? 1 : -1);
                Scope.Assign(name, value);
                Stack.Push(new StackFrame(instruction.Root, value));
                Advance();
                break;
            } 

            case OpCode.AND:
            case OpCode.OR:
            {
                var right = Convert.ToBoolean(Stack.Pop().Value);
                var left = Convert.ToBoolean(Stack.Pop().Value);
                var calculate = BinaryTypeOperations.Bool.GetValueOrDefault(instruction.OpCode);
                if (calculate == null)
                    Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                        $"Unhandled binary bool operation for op-code {instruction.OpCode}",
                        instruction.Root?.GetFirstToken());

                var result = calculate(left, right);
                Stack.Push(new StackFrame(instruction.Root, result));
                Advance();
                break;
            }
            
            case OpCode.EQ:
            case OpCode.NEQ:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var equalityComparer = EqualityComparer<object>.Default;
                var result = equalityComparer.Equals(left.Value, right.Value);
                if (instruction.OpCode == OpCode.NEQ)
                    result = !result;
                
                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.NOT:
            {
                var operand = Stack.Pop();
                var result = !Convert.ToBoolean(operand.Value);

                Stack.Push(new StackFrame(operand.Node, result));
                Advance();
                break;
            }
            
            case OpCode.BNOT:
            {
                var operand = Stack.Pop();
                var result = ~Convert.ToInt64(operand.Value);

                Stack.Push(new StackFrame(operand.Node, result));
                Advance();
                break;
            }

            case OpCode.JMP:
            {
                JumpUsingOffsetOperand(instruction);
                break;
            }
            case OpCode.JNZ:
            {
                var frame = Stack.Pop();
                if (frame.Value is not 0 and not false)
                    JumpUsingOffsetOperand(instruction);
                else
                    Advance();
                
                break;
            }
            case OpCode.JZ:
            {
                var frame = Stack.Pop();
                if (frame.Value is 0 or false)
                    JumpUsingOffsetOperand(instruction);
                else
                    Advance();
                
                break;
            }
            
            default:
            {
                Diagnostics.RuntimeError(DiagnosticCode.HDEV,
                    $"Unhandled opcode \"{instruction.OpCode}\"",
                    instruction.Root?.GetFirstToken());
                
                break;
            }
        }

        return null;
    }

    /// <returns>Whether the current instruction at the given pointer is the last meaningful one before a RETURN instruction</returns>
    private static bool IsLastOperationBeforeReturn(IReadOnlyList<Instruction> instructions, int pointer) =>
        IsLastOperation(instructions, pointer, OpCode.RETURN);
    
    /// <returns>Whether the current instruction at the given pointer is the last meaningful one before the given terminator</returns>
    private static bool IsLastOperation(IReadOnlyList<Instruction> instructions, int pointer, OpCode terminator = OpCode.EXIT)
    {
        var instruction = instructions.ElementAtOrDefault(pointer + 1);
        return instruction != null && instruction.OpCode == terminator;
    }
    
    /// <summary>Jumps ahead by the amount provided in the instruction's operand</summary>
    /// <exception cref="DiagnosticCode.H001C">If the given instruction's operand is not an <see cref="int"/></exception>
    private void JumpUsingOffsetOperand(Instruction instruction)
    {
        CheckNonIntegerOperand(instruction);
        if (instruction.Operand is int offset)
            Advance(offset);
    }

    /// <exception cref="DiagnosticCode.H001C">If the given instruction's operand is not an <see cref="int"/></exception>
    private void CheckNonIntegerOperand(Instruction instruction)
    {
        if (instruction.Operand is int) return;
        Diagnostics.RuntimeError(DiagnosticCode.H001C,
            $"Invalid bytecode! {instruction.OpCode} opcode was used with non-integer operand. Got: {instruction.Operand?.GetType().ToString() ?? "null"}",
            instruction.Root?.GetFirstToken());
    }

    private StackFrame CreateStackFrameFromInstruction(int offset = 0)
    {
        var instruction = _bytecode[_pointer + offset];
        return new StackFrame(instruction.Root, instruction.Operand);
    }

    private void Advance(int amount = 1) => _pointer += amount;
    
    private void StackDump()
    {
        Console.WriteLine("Stack contents:");
        foreach (var frame in Stack.Reverse())
        {
            Console.WriteLine(frame.Value ?? "null");
        }
    }
}