using Heir.AST.Abstract;
using Heir.CodeGeneration;
using Heir.Runtime;
using Heir.Runtime.HookedExceptions;
using Heir.Runtime.Values;
using Heir.Syntax;

namespace Heir;

internal sealed class ExitMarker;

internal sealed class StackFrame(SyntaxNode node, object? value)
{
    public SyntaxNode Node { get; } = node;
    public object? Value { get; } = value;
}

public sealed class VirtualMachine
{
    public DiagnosticBag Diagnostics { get; }
    public Scope GlobalScope { get; }
    public Scope Scope { get; private set; }
    public int RecursionDepth { get; private set; }
    
    private const int _maxRecursionDepth = 300; // can only handle about this much for now 💀
    private readonly Stack<StackFrame> _stack = [];
    private readonly Bytecode _bytecode;
    private Scope _enclosingScope;
    private int _pointer;

    public VirtualMachine(Bytecode bytecode, Scope? scope = null, int recursionDepth = 0)
    {
        Diagnostics = bytecode.Diagnostics;
        GlobalScope = new Scope();
        Scope = scope ?? GlobalScope;
        _enclosingScope = Scope;
        _bytecode = bytecode;
        RecursionDepth = recursionDepth;
    }

    public T? Evaluate<T>() => (T?)Evaluate();

    public object? Evaluate()
    {
        var length = _bytecode.Count;
        while (_pointer < length)
        {
            var instruction = _bytecode[_pointer];
            var result = EvaluateInstruction(instruction);
            if (instruction.OpCode == OpCode.RETURN)
                return result?.Value;

            if (result?.Value is ExitMarker) break;
        }

        return _stack.TryPop(out var stackFrame)
            ? stackFrame.Value
            : null;
    }

    public void EndRecursion(int level = 1) => RecursionDepth -= level;
    public void BeginRecursion(Token token)
    {
        if (RecursionDepth++ < _maxRecursionDepth) return;
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
            
            case OpCode.RETURN:
                return _stack.Pop();

            case OpCode.BEGINSCOPE:
                _enclosingScope = Scope;
                Scope = new Scope(_enclosingScope);
                Advance();
                break;
            case OpCode.ENDSCOPE:
                Scope = _enclosingScope;
                _enclosingScope = Scope.Enclosing ?? Scope;
                Advance();
                break;

            case OpCode.CALL:
            {
                var calleeFrame = _stack.Pop();
                if (instruction.Operand is not List<List<Instruction>> argumentsBytecode)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        "Failed to execute CALL op-code: Provided operand is not a list of argument bytecodes",
                        calleeFrame.Node.GetFirstToken());
                    
                    Advance();
                    break;
                }
                if (calleeFrame.Value is not Function function)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        "Failed to execute CALL op-code: Loaded callee is not a function",
                        calleeFrame.Node.GetFirstToken());
                    
                    Advance();
                    break;
                }

                var result = function.Call(this, argumentsBytecode);
                _stack.Push(new(calleeFrame.Node, result));
                Advance();
                break;
            }

            case OpCode.PUSH:
            case OpCode.PUSHNONE:
                _stack.Push(CreateStackFrameFromInstruction());
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
                            var keyVM = new VirtualMachine(new Bytecode(pair.Key, Diagnostics));
                            var valueVM = new VirtualMachine(new Bytecode(pair.Value, Diagnostics));
                            var key = keyVM.Evaluate()!;
                            var value = valueVM.Evaluate();
                            return new KeyValuePair<object, object?>(key, value);
                        })
                );

                _stack.Push(new StackFrame(bytecodeDictionaryFrame.Node, evaluatedDictionary));
                Advance();
                break;
            }

            case OpCode.POP:
            {
                _stack.Pop();
                Advance();
                break;
            }
            case OpCode.SWAP:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                _stack.Push(right);
                _stack.Push(left);
                Advance();
                break;
            }
            case OpCode.DUP:
            {
                var value = _stack.Peek();
                _stack.Push(value);
                Advance();
                break;
            }

            case OpCode.LOAD:
            {
                var nameFrame = _stack.Pop();
                if (nameFrame.Value is not string name)
                {
                    Diagnostics.Error(DiagnosticCode.HDEV,
                        $"Failed to execute LOAD op-code: No variable name was located in the stack, got {nameFrame.Value ?? "none"}",
                        nameFrame.Node.GetFirstToken());
                    
                    Advance();
                    break;
                }

                var value = Scope.Lookup(name);
                _stack.Push(new StackFrame(nameFrame.Node, value));
                Advance();
                break;
            }
            case OpCode.STORE:
            {
                var initializer = _stack.Pop();
                var nameFrame = _stack.Pop();
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
                    _stack.Push(initializer);

                Advance();
                break;
            }

            case OpCode.CONCAT:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToString(left.Value) + Convert.ToString(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.ADD:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) + Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.SUB:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) - Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.MUL:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) * Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.DIV:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.IDIV:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToInt64(Math.Floor(Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value)));

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.MOD:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) % Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.POW:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Math.Pow(Convert.ToDouble(left.Value), Convert.ToDouble(right.Value));

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BAND:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToInt64(left.Value) & Convert.ToInt64(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BOR:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToInt64(left.Value) | Convert.ToInt64(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BXOR:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToInt64(left.Value) ^ Convert.ToInt64(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BSHL:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToInt64(Convert.ToInt32(left.Value) << Convert.ToInt32(right.Value));

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.BSHR:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToInt64(Convert.ToInt32(left.Value) >> Convert.ToInt32(right.Value));

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.UNM:
            {
                var operand = _stack.Pop();
                var result = -Convert.ToDouble(operand.Value);

                _stack.Push(new StackFrame(operand.Node, result));
                Advance();
                break;
            }

            case OpCode.AND:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToBoolean(left.Value) && Convert.ToBoolean(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.OR:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToBoolean(left.Value) || Convert.ToBoolean(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.EQ:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var equalityComparer = EqualityComparer<object>.Default;

                _stack.Push(new StackFrame(right.Node, equalityComparer.Equals(left.Value, right.Value)));
                Advance();
                break;
            }
            case OpCode.LT:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) < Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }
            case OpCode.LTE:
            {
                var right = _stack.Pop();
                var left = _stack.Pop();
                var result = Convert.ToDouble(left.Value) <= Convert.ToDouble(right.Value);

                _stack.Push(new StackFrame(right.Node, result));
                Advance();
                break;
            }

            case OpCode.NOT:
            {
                var operand = _stack.Pop();
                var result = !Convert.ToBoolean(operand.Value);

                _stack.Push(new StackFrame(operand.Node, result));
                Advance();
                break;
            }
            case OpCode.BNOT:
            {
                var operand = _stack.Pop();
                var result = ~Convert.ToInt64(operand.Value);

                _stack.Push(new StackFrame(operand.Node, result));
                Advance();
                break;
            }

            case OpCode.JMP:
            {
                if (instruction.Operand is int index)
                    _pointer = index;
                else
                    NonIntegerOperand(instruction);

                break;
            }
            case OpCode.JNZ:
            {
                var frame = _stack.Pop();
                if (frame.Value is int n && n != 0)
                {
                    if (instruction.Operand is int index)
                        _pointer = index;
                    else
                        NonIntegerOperand(instruction);
                }
                else
                {
                    Advance();
                }

                break;
            }
            case OpCode.JZ:
            {
                var frame = _stack.Pop();
                if (frame.Value is 0)
                {
                    if (instruction.Operand is int index)
                        _pointer = index;
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

    private void Advance(int amount = 1) => _pointer += amount;
}