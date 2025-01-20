using Heir.CodeGeneration;

namespace Heir.Tests;

public class BytecodeSerdeTest
{
    [Fact]
    public void Serde_BasicInstructions()
    {
        var bytecode = new Bytecode([
            new(null, OpCode.PUSH, 6),
            new(null, OpCode.PUSH, 12),
            new(null, OpCode.ADD),
            new(null, OpCode.POP)
        ]);

        using var stream = new MemoryStream();
        BytecodeSerializer.Serialize(bytecode, stream, true);
        stream.Position = 0;
            
        var deserializedBytecode = BytecodeDeserializer.Deserialize(stream);
        for (var i = 0; i < bytecode.Count; i++)
        {
            var instruction = bytecode[i];
            var deserializedInstruction = deserializedBytecode.Instructions.ElementAtOrDefault(i);
            Assert.NotNull(deserializedInstruction);
            Assert.Equal(instruction.OpCode, deserializedInstruction.OpCode);
            Assert.Equal(instruction.Operand, deserializedInstruction.Operand);
        }
    }
}