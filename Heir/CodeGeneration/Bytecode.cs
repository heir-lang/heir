namespace Heir.CodeGeneration
{
    public class Bytecode(List<Instruction> instructions, DiagnosticBag diagnostics)
    {
        public IReadOnlyList<Instruction> Instructions => instructions.AsReadOnly();
        public DiagnosticBag Diagnostics { get; } = diagnostics;
        public int Count => Instructions.Count;

        public Instruction this[int index]
        {
            get => Instructions[index];
        }

        public Bytecode Skip(int count) => new Bytecode(instructions.Skip(count).ToList(), Diagnostics);
    }
}
