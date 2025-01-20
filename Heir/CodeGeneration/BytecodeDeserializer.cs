using System.Runtime.Serialization;
using System.Text;

namespace Heir.CodeGeneration;

public static class BytecodeDeserializer
{
    public static Bytecode Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        var version = reader.ReadByte();
        var instructions = DeserializeBytecodeOperand(reader);
        
        return new Bytecode(instructions)
        {
            Version = version
        };
    }

    private static object? DeserializeOperand(BinaryReader reader)
    {
        var operandType = (OperandType)reader.ReadByte();
        
        switch (operandType)
        {
            case OperandType.String:
            {
                var length = reader.ReadInt32();
                var bytes = reader.ReadBytes(length);
                return new string(Encoding.UTF8.GetChars(bytes));
            }
            case OperandType.Char:
            {
                var codepoint = reader.ReadByte();
                return Encoding.UTF8.GetChars([codepoint]).First();
            }
            case OperandType.Number:
                return reader.ReadInt64();
            case OperandType.Bool:
                return Convert.ToBoolean(reader.ReadByte());
            case OperandType.Bytecode:
                return DeserializeBytecodeOperand(reader);

            case OperandType.List:
            {
                var length = reader.ReadInt32();
                
                var list = new List<object?>();
                for (var i = 0; i < length; i++)
                    list.Add(DeserializeOperand(reader));

                return list;
            }
            case OperandType.IntStringListTuple:
            {
                var item1 = DeserializeOperand(reader);
                var item2 = DeserializeOperand(reader);
                return (item1, item2);
            }
            
            case OperandType.Null:
                return null;
            
            default:
                throw new SerializationException($"Failed to deserialize operand: Unknown operand type '{operandType}'");
        }
    }

    private static IReadOnlyList<Instruction> DeserializeBytecodeOperand(BinaryReader reader)
    {
        var instructionCount = reader.ReadInt32();
        
        var instructions = new List<Instruction>();
        for (var i = 0; i < instructionCount; i++)
        {
            var opCode = (OpCode)reader.ReadByte();
            var operand = DeserializeOperand(reader);
            instructions.Add(new Instruction(null, opCode, operand));
        }

        return instructions;
    }
}