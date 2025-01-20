using Heir.CodeGeneration;

namespace Heir.Tests;

public class BytecodeSerdeTest
{
    [Fact]
    public void Serde_Proc()
    {
        var bytecode = new Bytecode([
            new(null, OpCode.PROC, new List<Instruction>([
                new(null, OpCode.PUSH, 69),
                new(null, OpCode.POP)
            ]))
        ]);

        using var stream = new MemoryStream();
        BytecodeSerializer.Serialize(bytecode, stream, true);
        stream.Position = 0;
            
        var deserializedBytecode = BytecodeDeserializer.Deserialize(stream);
        AssertBytecodeEqual(bytecode, deserializedBytecode);
    }

    [Fact]
    public void Serde_BasicInstructions()
    {
        var bytecode = new Bytecode([
            new(null, OpCode.PUSH, 6),
            new(null, OpCode.PUSH, 12),
            new(null, OpCode.ADD),
            new(null, OpCode.POP),
            new(null, OpCode.PUSH, "abc")
        ]);

        using var stream = new MemoryStream();
        BytecodeSerializer.Serialize(bytecode, stream, true);
        stream.Position = 0;
            
        var deserializedBytecode = BytecodeDeserializer.Deserialize(stream);
        AssertBytecodeEqual(bytecode, deserializedBytecode);
    }
    
    private static void AssertBytecodeEqual(Bytecode bytecode, Bytecode deserializedBytecode)
    {
        for (var i = 0; i < bytecode.Count; i++)
        {
            var instruction = bytecode[i];
            var deserializedInstruction = deserializedBytecode.Instructions.ElementAtOrDefault(i);
            Assert.NotNull(deserializedInstruction);
            Assert.Equal(instruction.OpCode, deserializedInstruction.OpCode);
            if (instruction.Operand is List<Instruction> instructions && deserializedInstruction.Operand is List<Instruction> deserializedInstructions)
                AssertBytecodeEqual(new(instructions), new(deserializedInstructions));
            else
                Assert.Equal(instruction.Operand, deserializedInstruction.Operand);
        }
    }
}