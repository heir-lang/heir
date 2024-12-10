using Heir.CodeGeneration;
using System.Numerics;

namespace Heir
{
    public class VirtualMachine(DiagnosticBag diagnostics, List<Instruction> bytecode)
    {
        public DiagnosticBag Diagnostics { get; } = diagnostics;

        private readonly List<Instruction> _bytecode = bytecode;
        private readonly Stack<object?> _stack = new();
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
                    return result;
            }

            return _stack.Pop();
        }

        private object? EvaluateInstruction(Instruction instruction)
        {
            switch (instruction.OpCode)
            {
                case OpCode.EXIT:
                    break;
                case OpCode.NOOP:
                    Advance();
                    break;
                case OpCode.RETURN:
                    return _stack.Pop();

                case OpCode.PUSH:
                    _stack.Push(GetValueFromMemory());
                    Advance();
                    break;
                case OpCode.PUSHNONE:
                    _stack.Push(null);
                    Advance();
                    break;
                case OpCode.DUP:
                    {
                        var value = _stack.Peek();
                        _stack.Push(value);
                        Advance();
                        break;
                    }

                case OpCode.ADD:
                    {
                        // TODO: use bound node's type to convert value
                        var right = Convert.ToDouble(_stack.Pop());
                        var left = Convert.ToDouble(_stack.Pop());
                        _stack.Push(left + right);
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
                        var value = _stack.Pop();
                        if (value is int n && n != 0)
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
                        var value = _stack.Pop();
                        if (value is int n && n == 0)
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
            }

            return null;
        }

        private void NonIntegerOperand(Instruction instruction)
        {
            Diagnostics.Error("H001C", $"Invalid bytecode! {instruction.OpCode} opcode was used with non-integer operand.", instruction.Root.GetFirstToken());
        }

        private object? GetValueFromMemory(int offset = 0)
        {
            var instruction = _bytecode[_pointer + offset];
            return instruction.Operand;
        }

        private T? GetValueFromMemory<T>(int offset = 0)
        {
            return (T?)GetValueFromMemory(offset);
        }

        private void Advance(int amount = 1)
        {
            _pointer += amount;
        }
    }
}
