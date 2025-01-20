namespace Heir.CodeGeneration;

public class Bytecode(IEnumerable<Instruction> instructions)
{
    public int Version { get; } = 1;
    public IReadOnlyList<Instruction> Instructions { get; } = instructions.ToList().AsReadOnly();
    public int Count => Instructions.Count;

    public Instruction this[int index] => Instructions[index];

    public Bytecode Skip(int count) => new(instructions.Skip(count));
    public Bytecode SkipLast(int count) => new(instructions.SkipLast(count));
    public Bytecode Take(int count) => new(instructions.Take(count));
    public Bytecode TakeLast(int count) => new(instructions.TakeLast(count));
    
    public override string ToString() =>
        string.Join('\n', Instructions.Select(instruction => instruction.ToString()));
}