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
        switch (Operand)
        {
            case List<Instruction> rawBytecode:
            {
                var bytecode = new Bytecode(rawBytecode);
                return $"{OpCode} (bytecode){(Root == null ? "" : " - " + Root.GetFirstToken().Span.Start)}\n"
                       + string.Join('\n', bytecode.ToString().Split('\n').Select(line => "  " + line));
            }
            case Dictionary<List<Instruction>, List<Instruction>> objectBytecode:
            {
                var final = $"{OpCode} (object bytecode){(Root == null ? "" : " - " + Root.GetFirstToken().Span.Start)}\n";
                var index = 0;
                foreach (var (keyBytecode, valueBytecode) in objectBytecode)
                {
                    final += "  Key:\n";
                    final += string.Join('\n', new Bytecode(keyBytecode).ToString().Split('\n').Select(line => "    " + line));
                    final += "\n";
                    final += "  Value:\n";
                    final += string.Join('\n', new Bytecode(valueBytecode).ToString().Split('\n').Select(line => "    " + line));
                    final += "\n";
                    if (index++ < objectBytecode.Count - 1)
                        final += "\n";
                }
            
                return final;
            }
            
            default:
                return Operand != null
                    ? $"{OpCode} {Operand}{(Root == null ? "" : " - " + Root.GetFirstToken().Span.Start)}"
                    : OpCode.ToString();
        }
    }
}