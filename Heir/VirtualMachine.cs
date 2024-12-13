using Heir.AST;
using Heir.CodeGeneration;
using Heir.Types;

namespace Heir
{
    sealed class StackFrame(SyntaxNode node, object? value)
    {
        public SyntaxNode Node { get; } = node;
        public object? Value { get; } = value;
    }

    public sealed class VirtualMachine(Binder binder, List<Instruction> bytecode)
    {
        public DiagnosticBag Diagnostics { get; } = binder.Diagnostics;

        private readonly Binder _binder = binder;
        private readonly List<Instruction> _bytecode = bytecode;
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
            }

            return _stack.Pop().Value;
        }

        private StackFrame? EvaluateInstruction(Instruction instruction)
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

                case OpCode.ADD:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var rightBoundNode = _binder.GetBoundNode((Expression)right.Node);
                        var leftBoundNode = _binder.GetBoundNode((Expression)left.Node);
                        object? result = null;

                        if (leftBoundNode.Type.IsAssignableTo(IntrinsicTypes.Number) && rightBoundNode.Type.IsAssignableTo(IntrinsicTypes.Number))
                            result = Convert.ToDouble(left.Value) + Convert.ToDouble(right.Value);
                        else if (leftBoundNode.Type.IsAssignableTo(PrimitiveType.String) && rightBoundNode.Type.IsAssignableTo(PrimitiveType.String))
                            result = Convert.ToString(left.Value) + Convert.ToString(right.Value);
                        else if (leftBoundNode.Type.IsAssignableTo(PrimitiveType.Char) && rightBoundNode.Type.IsAssignableTo(PrimitiveType.Char))
                            result = Convert.ToChar(left.Value) + Convert.ToChar(right.Value);

                        _stack.Push(new(right.Node, result));
                        Advance();
                        break;
                    }

                case OpCode.ADD:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();
                        var rightBoundNode = _binder.GetBoundNode((Expression)right.Node);
                        var leftBoundNode = _binder.GetBoundNode((Expression)left.Node);
                        object? result = null;

                        if (leftBoundNode.Type.IsAssignableTo(IntrinsicTypes.Number) && rightBoundNode.Type.IsAssignableTo(IntrinsicTypes.Number))
                            result = Convert.ToDouble(left.Value) + Convert.ToDouble(right.Value);
                        else if (leftBoundNode.Type.IsAssignableTo(PrimitiveType.String) && rightBoundNode.Type.IsAssignableTo(PrimitiveType.String))
                            result = Convert.ToString(left.Value) + Convert.ToString(right.Value);
                        else if (leftBoundNode.Type.IsAssignableTo(PrimitiveType.Char) && rightBoundNode.Type.IsAssignableTo(PrimitiveType.Char))
                            result = Convert.ToChar(left.Value) + Convert.ToChar(right.Value);

                        _stack.Push(new(right.Node, result));
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
            }

            return null;
        }

        private void NonIntegerOperand(Instruction instruction)
        {
            Diagnostics.Error("H001C", $"Invalid bytecode! {instruction.OpCode} opcode was used with non-integer operand.", instruction.Root.GetFirstToken());
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
