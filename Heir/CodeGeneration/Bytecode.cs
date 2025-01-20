namespace Heir.CodeGeneration;

public sealed class Bytecode(IEnumerable<Instruction> instructions)
{
    public byte Version { get; init; } = 1;
    public IReadOnlyList<Instruction> Instructions { get; } = instructions.ToList().AsReadOnly();
    public int Count => Instructions.Count;

    public Instruction this[int index] => Instructions[index];

    public Bytecode Skip(int count) => new(instructions.Skip(count));
    public Bytecode SkipLast(int count) => new(instructions.SkipLast(count));
    public Bytecode Take(int count) => new(instructions.Take(count));
    public Bytecode TakeLast(int count) => new(instructions.TakeLast(count));
    public bool Contains(List<Instruction> section) =>Utility.ContainsSequence(Instructions, section);

    public bool Equals(Bytecode other) => Equals(other.Instructions);
    
    public bool Equals(List<Instruction> other)
    {
        var index = 0;
        var matches = true;
        foreach (var instruction in Instructions)
        {
            var otherInstruction = other.ElementAtOrDefault(index++);
            if (otherInstruction is null)
                return false;
            
            matches = matches && instruction.Equals(otherInstruction);
            if (!matches) break; // reduce redundant iteration
        }

        return matches;
    }
    
    public override string ToString() =>
        string.Join('\n', Instructions.Select(instruction => instruction.ToString()));
}