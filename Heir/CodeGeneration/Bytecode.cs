namespace Heir.CodeGeneration;

public class Bytecode(IEnumerable<Instruction> instructions, DiagnosticBag diagnostics)
{
    public IReadOnlyList<Instruction> Instructions => instructions.ToList().AsReadOnly();
    public DiagnosticBag Diagnostics { get; } = diagnostics;
    public int Count => Instructions.Count;

    public Instruction this[int index] => Instructions[index];

    public Bytecode Skip(int count) =>
        new(instructions.Skip(count).ToList(), Diagnostics);
    
    public Bytecode SkipLast(int count) =>
        new(instructions.SkipLast(count).ToList(), Diagnostics);

    public Bytecode Take(int count) =>
        new(instructions.Take(count).ToList(), Diagnostics);
    
    public Bytecode TakeLast(int count) =>
        new(instructions.TakeLast(count).ToList(), Diagnostics);
    
    public override string ToString() =>
        string.Join('\n', Instructions.Select(instruction => instruction.ToString()));
}