using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace Heir.CodeGeneration;

public static class BytecodeSerializer
{
    public static void Serialize(Bytecode bytecode, Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        writer.Write(bytecode.Version);
        SerializeBytecodeOperand(bytecode.Instructions, writer);
    }
    
    private static void SerializeOperand(object? operand, BinaryWriter writer)
    {
        switch(operand)
        {
            case string s:
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(s);
                writer.Write((byte)OperandType.String);
                writer.Write(utf8Bytes.Length);
                writer.Write(utf8Bytes);
                break;
            }
            case char c:
            {
                var utf8Bytes = Encoding.UTF8.GetBytes([c]);
                writer.Write((byte)OperandType.String);
                writer.Write(utf8Bytes.First());
                break;
            }
            case long or ulong:
                writer.Write((byte)OperandType.Long);
                writer.Write(Convert.ToInt64(operand));
                break;
            case int or uint or short or ushort or byte or sbyte:
                writer.Write((byte)OperandType.Int);
                writer.Write(Convert.ToInt32(operand));
                break;
           case double or float or decimal:
                writer.Write((byte)OperandType.Double);
                writer.Write(Convert.ToDouble(operand));
                break;
            case bool:
                writer.Write((byte)OperandType.Bool);
                writer.Write(Convert.ToByte(operand));
                break;
            case List<Instruction> bytecode:
                writer.Write((byte)OperandType.Bytecode);
                SerializeBytecodeOperand(bytecode, writer);
                break;
            case ValueTuple<int, List<string>> tuple:
            {
                writer.Write((byte)OperandType.IntStringListTuple);
                SerializeOperand(tuple.Item1, writer);
                SerializeOperand(tuple.Item2, writer);
                break;
            }
            case List<string> list:
                writer.Write((byte)OperandType.StringList);
                writer.Write(list.Count);
                foreach (var element in list)
                    SerializeOperand(element, writer);
                
                break;
            case null:
                writer.Write((byte)OperandType.Null);
                break;
            
            default:
                throw new SerializationException($"Failed to serialize operand: Unknown operand type '{operand.GetType()}'");
        }
    }
    
    private static void SerializeBytecodeOperand(IReadOnlyList<Instruction> bytecode, BinaryWriter writer)
    {
        writer.Write(bytecode.Count);

        foreach (var instruction in bytecode)
        {
            writer.Write((byte)instruction.OpCode);
            SerializeOperand(instruction.Operand, writer);
        }
    }
}