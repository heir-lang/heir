using Heir.AST;
using Heir.AST.Abstract;
using Heir.CodeGeneration;
using Heir.Runtime;
using Heir.Runtime.HookedExceptions;
using Heir.Runtime.Intrinsics;
using Heir.Runtime.Values;
using Heir.Syntax;
using Spectre.Console;

namespace Heir;

internal sealed class ExitMarker;

public sealed class StackFrame(SyntaxNode node, object? value)
{
    public SyntaxNode Node { get; } = node;
    public object? Value { get; } = value;
}

internal sealed class CallStackFrame(Bytecode bytecode, Scope closure, int enclosingPointer)
{
    public Bytecode Bytecode { get; } = bytecode;
    public Scope Closure { get; } = closure;
    public int EnclosingPointer { get; } = enclosingPointer;
}

public sealed class VirtualMachine
{
    public DiagnosticBag Diagnostics { get; }
    public Scope GlobalScope { get; }
    public Scope Scope { get; private set; }
    public Stack<StackFrame> Stack { get; } = [];
    private int _recursionDepth;
    
    private const int _maxRecursionDepth = 20000;
    private readonly Stack<CallStackFrame> _callStack = [];

    private Bytecode _bytecode;
    private Scope _enclosingScope;
    private int _pointer;

    public VirtualMachine(Bytecode bytecode, Scope? scope = null, int recursionDepth = 0)
    {
        Diagnostics = bytecode.Diagnostics;
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
        Diagnostics.Error(DiagnosticCode.H017, $"Stack overflow: Recursion depth of {_maxRecursionDepth} exceeded", token);
        throw new Exception();
    }
    
    private StackFrame? EvaluateInstruction(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.EXIT:
                return new StackFrame(instruction.Root, new ExitMarker());
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
                if (instruction.Root is not FunctionDeclaration functionDeclaration)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        "Failed to execute PROC op-code: Provided node is not a FunctionDeclaration",
                        instruction.Root.GetFirstToken());
                    
                    Advance();
                    break;
                }

                if (instruction.Operand is not List<Instruction> bodyBytecode)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        "Failed to execute PROC op-code: Provided operand is not the function body's bytecode",
                        functionDeclaration.GetFirstToken());
                    
                    Advance();
                    break;
                }
                
                var function = new FunctionValue(functionDeclaration, bodyBytecode, new Scope(Scope));
                Stack.Push(new(functionDeclaration, function));
                Advance();
                break;
            }
            case OpCode.CALL:
            {
                if (instruction.Operand is not ValueTuple<int, List<string>> data)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        $"Failed to execute CALL op-code: Provided operand is not an tuple containing parameter names & the amount of argument instructions.\nGot: {Markup.Escape(instruction.Operand?.GetType().ToString() ?? "null")}",
                        instruction.Root.GetFirstToken());
                    
                    break;
                }

                var (argumentInstructionsCount, parameterNames) = data;
                var argumentsBytecode = _bytecode.Skip(_pointer + 1).Take(argumentInstructionsCount);
                var argumentVM = new VirtualMachine(argumentsBytecode, Scope, _recursionDepth);
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
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        $"Failed to execute CALL op-code: Loaded callee is not a function, got {Markup.Escape(calleeFrame.Value?.GetType().ToString() ?? "null")}.\nCallee frame node: {calleeFrame.Node}",
                        calleeFrame.Node.GetFirstToken());
                
                if (calleeFrame.Value is FunctionValue function)
                {
                    Advance(argumentInstructionsCount);
                    List<Instruction> bodyBytecode =
                    [
                        new(function.Declaration, OpCode.BEGINSCOPE),
                        ..argumentDefinitionBytecode,
                        ..function.BodyBytecode.Skip(1)
                    ];

                    var isTailCall = _callStack.TryPeek(out var currentState) &&
                                  currentState.EnclosingPointer == _pointer + 1;
                    
                    if (!isTailCall)
                        _callStack.Push(new(_bytecode, Scope, _pointer + 1));

                    BeginRecursion(function.Declaration.Name.Token);
                    _bytecode = new Bytecode(bodyBytecode, Diagnostics);
                    _pointer = 0;
                    Scope = function.Closure;
                }
                else if (calleeFrame.Value is IntrinsicFunction intrinsicFunction)
                {
                    var argumentValues = argumentVM.Stack
                        .TakeLast(argumentsBytecode.Count)
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
                if (objectFrame.Value is not Dictionary<object, object?> objectDictionary)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        "Failed to execute INDEX op-code: Loaded object is not an object dictionary",
                        objectFrame.Node.GetFirstToken());
                    
                    break;
                }
                if (indexFrame.Value is null)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        "Failed to execute INDEX op-code: Loaded index is null",
                        objectFrame.Node.GetFirstToken());
                    
                    break;
                }
                
                var value = objectDictionary[indexFrame.Value];
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
                            var keyVM = new VirtualMachine(new Bytecode(pair.Key, Diagnostics), Scope);
                            var valueVM = new VirtualMachine(new Bytecode(pair.Value, Diagnostics), Scope);
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
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        $"Failed to execute LOAD op-code: No variable name was located in the stack, got {nameFrame.Value ?? "none"}",
                        nameFrame.Node.GetFirstToken());
                    
                    Advance();
                    break;
                }

                var value = Scope.Lookup(name);
                Stack.Push(new StackFrame(nameFrame.Node, value));
                Advance();
                break;
            }
            case OpCode.STORE:
            {
                var initializer = Stack.Pop();
                var nameFrame = Stack.Pop();
                if (nameFrame.Value is not string name)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        $"Failed to execute STORE op-code: No variable name was located in the stack, got {nameFrame.Value ?? "none"}",
                        initializer.Node.GetFirstToken());
                    
                    Advance();
                    break;
                }

                if (Scope.IsDeclared(name))
                    Scope.Assign(name, initializer.Value);
                else
                    Scope.Define(name, initializer.Value);

                if (instruction.Operand is true)
                    Stack.Push(initializer);

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
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) + Convert.ToDouble(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.SUB:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) - Convert.ToDouble(right.Value);
                
                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.MUL:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) * Convert.ToDouble(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.DIV:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.IDIV:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToInt64(Math.Floor(Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value)));

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.MOD:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) % Convert.ToDouble(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.POW:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Math.Pow(Convert.ToDouble(left.Value), Convert.ToDouble(right.Value));

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BAND:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToInt64(left.Value) & Convert.ToInt64(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BOR:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToInt64(left.Value) | Convert.ToInt64(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BXOR:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToInt64(left.Value) ^ Convert.ToInt64(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BSHL:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToInt64(Convert.ToInt32(left.Value) << Convert.ToInt32(right.Value));

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BSHR:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToInt64(Convert.ToInt32(left.Value) >> Convert.ToInt32(right.Value));

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.UNM:
            {
                var operand = Stack.Pop();
                var result = -Convert.ToDouble(operand.Value);

                Stack.Push(new StackFrame(operand.Node, result));
                Advance();
                break;
            }

            case OpCode.AND:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToBoolean(left.Value) && Convert.ToBoolean(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.OR:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToBoolean(left.Value) || Convert.ToBoolean(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.EQ:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var equalityComparer = EqualityComparer<object>.Default;

                Stack.Push(new StackFrame(right.Node, equalityComparer.Equals(left.Value, right.Value)));
                Advance();
                break;
            }
            case OpCode.LT:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) < Convert.ToDouble(right.Value);

                Stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.LTE:
            {
                var right = Stack.Pop();
                var left = Stack.Pop();
                var result = Convert.ToDouble(left.Value) <= Convert.ToDouble(right.Value);

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
                if (instruction.Operand is int offset)
                    Advance(offset);
                else
                    NonIntegerOperand(instruction);

                break;
            }
            case OpCode.JNZ:
            {
                var frame = Stack.Pop();
                if (frame.Value is not 0 and not false)
                {
                    if (instruction.Operand is int offset)
                        Advance(offset);
                    else
                        NonIntegerOperand(instruction);
                }
                else
                    Advance();
                
                break;
            }
            case OpCode.JZ:
            {
                var frame = Stack.Pop();
                if (frame.Value is 0 or false)
                {
                    if (instruction.Operand is int offset)
                        Advance(offset);
                    else
                        NonIntegerOperand(instruction);
                }
                else
                    Advance();
                
                break;
            }
            
            default:
            {
                Diagnostics.Error(DiagnosticCode.H001D,
                    $"Unhandled opcode \"{instruction.OpCode}\"",
                    instruction.Root.GetFirstToken());
                
                return new StackFrame(instruction.Root, new ExitMarker());
            }
        }

        return null;
    }

    private void NonIntegerOperand(Instruction instruction) =>
        Diagnostics.Error(DiagnosticCode.H001C,
            $"Invalid bytecode! {instruction.OpCode} opcode was used with non-integer operand.",
            instruction.Root.GetFirstToken());

    private StackFrame CreateStackFrameFromInstruction(int offset = 0)
    {
        var instruction = _bytecode[_pointer + offset];
        return new StackFrame(instruction.Root, instruction.Operand);
    }
    
    private void StackDump()
    {
        Console.WriteLine("Stack contents:");
        foreach (var frame in Stack)
        {
            Console.WriteLine(frame.Value ?? "null");
        }
    }

    private void Advance(int amount = 1) => _pointer += amount;
}