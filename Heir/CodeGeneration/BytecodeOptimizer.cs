namespace Heir.CodeGeneration;

public class BytecodeOptimizer(List<Instruction> bytecode, DiagnosticBag diagnostics)
{
    private int _pointer;
    
    public List<Instruction> Optimize()
    {
        var optimizedBytecode = new List<Instruction>();
        foreach (var instruction in bytecode)
        {
            var optimizedInstruction = Optimize(instruction);
            if (optimizedInstruction == null)
            {
                optimizedBytecode.Add(instruction);
                continue;
            }
            
            optimizedBytecode.Add(optimizedInstruction);
            _pointer++;
        }

        return optimizedBytecode;
    }

    private Instruction? Optimize(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.PROC:
            {
                if (instruction.Operand is not List<Instruction> bodyBytecode)
                {
                    diagnostics.Error(DiagnosticCode.HDEV,
                        "Invalid PROC op-code: Provided operand is not the function body's bytecode (List<Instruction>)",
                        instruction.Root?.GetFirstToken());
                    
                    break;
                }
                
                var bodyOptimizer = new BytecodeOptimizer(bodyBytecode, diagnostics);
                return new Instruction(instruction.Root, instruction.OpCode, bodyOptimizer.Optimize());
            }
        }
        
        return null;
    }

    private Instruction? PeekBytecode(int offset = 0) =>
        bytecode.ElementAtOrDefault(_pointer + offset);
}