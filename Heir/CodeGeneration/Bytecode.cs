namespace Heir.CodeGeneration;

public class Bytecode(List<Instruction> instructions, DiagnosticBag diagnostics)
{
    public IReadOnlyList<Instruction> Instructions => instructions.AsReadOnly();
    public DiagnosticBag Diagnostics { get; } = diagnostics;
    public int Count => Instructions.Count;

    public Instruction this[int index] => Instructions[index];

    public Bytecode Skip(int count) =>
        new(instructions.Skip(count).ToList(), Diagnostics);

    public override string ToString() =>
        string.Join('\n', Instructions.Select(instruction => instruction.ToString()));
}