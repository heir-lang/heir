using Heir.AST;

namespace Heir.CodeGeneration
{
    public class Instruction(SyntaxNode node, OpCode opCode, object? operand = null)
        : Instruction<object>(node, opCode, operand)
    { 
    }

    public class Instruction<T>(SyntaxNode node, OpCode opCode, T? operand = default)
    {
        public SyntaxNode Root { get; } = node;
        public OpCode OpCode { get; } = opCode;
        public T? Operand { get; } = operand;

        public override string ToString() => Operand != null ? $"{OpCode} {Operand}" : OpCode.ToString();
    }
}
