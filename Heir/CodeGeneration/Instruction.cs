using Heir.AST.Abstract;

namespace Heir.CodeGeneration;

public class Instruction(SyntaxNode? root, OpCode opCode, object? operand = null)
{
    public SyntaxNode? Root { get; } = root;
    public OpCode OpCode { get; } = opCode;
    public object? Operand { get; } = operand;

    public Instruction WithRoot(SyntaxNode? root) => new(root, OpCode, Operand);
    public Instruction WithOpCode(OpCode opCode) => new(Root, opCode, Operand);
    public Instruction WithOperand(object? operand) => new(Root, OpCode, operand);

    public bool Equals(Instruction other) =>
        OpCode == other.OpCode &&
        Operand?.Equals(other.Operand) == true;

    public override string ToString()
    {
        if (Operand is List<Instruction> rawBytecode)
        {
            var bytecode = new Bytecode(rawBytecode);
            return $"{OpCode} (bytecode){(Root == null ? "" : " - " + Root.GetFirstToken().Span.Start)}\n"
                + string.Join('\n', bytecode.ToString().Split('\n').Select(line => "  " + line));
        }
        
        return Operand != null
            ? $"{OpCode} {Operand}{(Root == null ? "" : " - " + Root.GetFirstToken().Span.Start)}"
            : OpCode.ToString();
    }
}