using Heir.AST.Abstract;

namespace Heir.CodeGeneration;

public class Instruction(SyntaxNode? node, OpCode opCode, object? operand = null)
{
    public SyntaxNode? Root { get; } = node!;
    public OpCode OpCode { get; } = opCode;
    public object? Operand { get; } = operand;

    public override string ToString()
    {
        if (Operand is List<Instruction> rawBytecode)
        {
            var bytecode = new Bytecode(rawBytecode);
            return $"{OpCode} (bytecode){(Root == null ? "" : " - " + Root.GetFirstToken().StartLocation)}\n"
                + string.Join('\n', bytecode.ToString().Split('\n').Select(line => "  " + line));
        }
        
        return Operand != null
            ? $"{OpCode} {Operand}{(Root == null ? "" : " - " + Root.GetFirstToken().StartLocation)}"
            : OpCode.ToString();
    }
}