namespace Heir.CodeGeneration
{
    public class Instruction(OpCode opCode, int? operand = null)
    {
        public OpCode OpCode { get; } = opCode;
        public int? Operand { get; } = operand;

        public override string ToString() => Operand.HasValue ? $"{OpCode} {Operand}" : OpCode.ToString();
    }
}
