using Heir.AST;
using Heir.CodeGeneration;

namespace Heir
{
    sealed class ExitMarker;

    sealed class StackFrame(SyntaxNode node, object? value)
    {
        public SyntaxNode Node { get; } = node;
        public object? Value { get; } = value;
    }

    public sealed class VirtualMachine(Binder binder, Bytecode bytecode)
    {
        public DiagnosticBag Diagnostics { get; } = bytecode.Diagnostics;

        private readonly Binder _binder = binder;
        private readonly Bytecode _bytecode = bytecode;
        private readonly Stack<StackFrame> _stack = new();
        private int _pointer = 0;

        public T? Evaluate<T>()
        {
            return (T?)Evaluate();
        }

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

            return _stack.TryPop(out var stackFrame) ? stackFrame.Value : null;
        }

        private StackFrame? EvaluateInstruction(Instruction instruction)
        {
            switch (instruction.OpCode)
            {
                case OpCode.EXIT:
                    return new(instruction.Root, new ExitMarker());
                case OpCode.NOOP:
                    Advance();
                    break;
                case OpCode.RETURN:
                    return _stack.Pop();

                case OpCode.PUSH:
                case OpCode.PUSHNONE:
                    _stack.Push(CreateStackFrameFromInstruction());
                    Advance();
                    break;
                case OpCode.DUP:
                    {
                        var value = _stack.Peek();
                        _stack.Push(value);
                        Advance();
                        break;
                    }

                case OpCode.CONCAT:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToString(left.Value) + Convert.ToString(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.ADD:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) + Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.SUB:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) - Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.MUL:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) * Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.DIV:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.IDIV:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Math.Floor(Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value));

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.MOD:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) % Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.POW:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Math.Pow(Convert.ToDouble(left.Value), Convert.ToDouble(right.Value));

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.BAND:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToInt64(left.Value) & Convert.ToInt64(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.BOR:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToInt64(left.Value) | Convert.ToInt64(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.BXOR:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToInt64(left.Value) ^ Convert.ToInt64(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.AND:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToBoolean(left.Value) || Convert.ToBoolean(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.OR:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToBoolean(left.Value) || Convert.ToBoolean(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.EQ:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var equalityComparer = EqualityComparer<object>.Default;

                        _stack.Push(new(right.Node, equalityComparer.Equals(left.Value, right.Value)));
                        Advance();
                        break;
                    }
                case OpCode.LT:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) < Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.LTE:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var result = Convert.ToDouble(left.Value) <= Convert.ToDouble(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }

                case OpCode.NOT:
                    {
                        var operand = _stack.Pop();
                        var result = !Convert.ToBoolean(operand.Value);

                        _stack.Push(new(operand.Node, result));
                        Advance();
                        break;
                    }
                case OpCode.BNOT:
                    {
                        var operand = _stack.Pop();
                        var result = ~Convert.ToInt64(operand.Value);

                        _stack.Push(new(operand.Node, result));
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
                            Advance();
                        
                        break;
                    }
                case OpCode.JZ:
                    {
                        var frame = _stack.Pop();
                        if (frame.Value is int n && n == 0)
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
                        Diagnostics.Error(DiagnosticCode.H001D, $"Unhandled opcode \"{instruction.OpCode}\"", instruction.Root.GetFirstToken());
                        break;
                    }
            }

            return null;
        }

        private void NonIntegerOperand(Instruction instruction)
        {
            Diagnostics.Error(DiagnosticCode.H001C, $"Invalid bytecode! {instruction.OpCode} opcode was used with non-integer operand.", instruction.Root.GetFirstToken());
        }

        private StackFrame CreateStackFrameFromInstruction(int offset = 0)
        {
            var instruction = _bytecode[_pointer + offset];
            return new(instruction.Root, instruction.Operand);
        }

        private void Advance(int amount = 1)
        {
            _pointer += amount;
        }
    }
}
