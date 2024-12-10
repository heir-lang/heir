namespace Heir.CodeGeneration
{
    public class Instruction(OpCode opCode, object? operand = null) : Instruction<object>(opCode, operand)
    { 
    }

    public class Instruction<T>(OpCode opCode, T? operand = default)
    {
        public OpCode OpCode { get; } = opCode;
        public T? Operand { get; } = operand;

        public override string ToString() => Operand != null ? $"{OpCode} {Operand}" : OpCode.ToString();
    }
}
