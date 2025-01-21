using Heir.Runtime;

namespace Heir.CodeGeneration;

public class BytecodeOptimizer(List<Instruction> bytecode, DiagnosticBag diagnostics)
{
    private int _pointer;
    
    public List<Instruction> Optimize()
    {
        var optimizedBytecode = new List<Instruction>();
        while (_pointer < bytecode.Count)
        {
            var instruction = PeekBytecode()!;
            var optimizedInstruction = Optimize(instruction);

            if (optimizedInstruction == null)
            {
                optimizedBytecode.Add(instruction);
                Advance();
                continue;
            }
            
            optimizedBytecode.Add(optimizedInstruction);
        }

        return optimizedBytecode;
    }

    private Instruction? Optimize(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.PUSH:
            {
                // pre-compute the result of operations using only literal values
                // FUCK THIS CODE
                
                // unary
                {
                    if (instruction.Operand is long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.UNM or OpCode.BNOT
                        } operation)
                    {
                        var result = operation.OpCode == OpCode.UNM
                            ? -Convert.ToDouble(instruction.Operand)
                            : ~Convert.ToInt64(instruction.Operand);

                        Advance();
                        Advance();
                        return instruction.WithOperand(result);
                    }
                }
                
                // binary
                {
                    if (instruction.Operand is string or char &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.PUSH,
                            Operand: string or char
                        } rightInstruction &&
                        PeekBytecode(2) is
                        {
                            OpCode: OpCode.CONCAT
                        })
                    {
                        var left = Convert.ToString(instruction.Operand);
                        var right = Convert.ToString(rightInstruction.Operand);

                        Advance(3);
                        return instruction.WithOperand(left + right);
                    }
                }
                {
                    if (instruction.Operand is long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.PUSH,
                            Operand: long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal
                        } rightInstruction &&
                        PeekBytecode(2) is { } operation &&
                        BinaryTypeOperations.Double.TryGetValue(operation.OpCode, out var calculate))
                    {
                        var left = Convert.ToDouble(instruction.Operand);
                        var right = Convert.ToDouble(rightInstruction.Operand);
                        var result = calculate(left, right);

                        Advance(3);
                        return instruction.WithOperand(result);
                    }
                }
                {
                    if (instruction.Operand is long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.PUSH,
                            Operand: long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal
                        } rightInstruction &&
                        PeekBytecode(2) is { } operation &&
                        BinaryTypeOperations.Long.TryGetValue(operation.OpCode, out var calculate))
                    {
                        var left = Convert.ToInt64(instruction.Operand);
                        var right = Convert.ToInt64(rightInstruction.Operand);
                        var result = calculate(left, right);

                        Advance(3);
                        return instruction.WithOperand(result);
                    }
                }
                {
                    if (instruction.Operand is long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.PUSH,
                            Operand: long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal
                        } rightInstruction &&
                        PeekBytecode(2) is { } operation &&
                        BinaryTypeOperations.Int.TryGetValue(operation.OpCode, out var calculate))
                    {
                        var left = Convert.ToInt32(instruction.Operand);
                        var right = Convert.ToInt32(rightInstruction.Operand);
                        var result = Convert.ToInt64(calculate(left, right));

                        Advance(3);
                        return instruction.WithOperand(result);
                    }
                }
                {
                    if (instruction.Operand is var leftRaw &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.PUSH,
                            Operand: var rightRaw
                        } &&
                        PeekBytecode(2) is { } operation &&
                        BinaryTypeOperations.Bool.TryGetValue(operation.OpCode, out var calculate))
                    {
                        var left = Convert.ToBoolean(leftRaw);
                        var right = Convert.ToBoolean(rightRaw);
                        var result = calculate(left, right);

                        Advance(3);
                        return instruction.WithOperand(result);
                    }
                }
                {
                    if (instruction.Operand is var left &&
                        PeekBytecode(1) is
                        {
                            OpCode: OpCode.PUSH,
                            Operand: var right
                        } &&
                        PeekBytecode(2) is
                        {
                            OpCode: OpCode.EQ or OpCode.NEQ
                        } operation)
                    {
                        var equalityComparer = EqualityComparer<object>.Default;
                        var result = equalityComparer.Equals(left, right);
                        if (operation.OpCode == OpCode.NEQ)
                            result = !result;

                        Advance(3);
                        return instruction.WithOperand(result);
                    }
                }
                
                break;
            }
            case OpCode.PROC:
            {
                Advance();
                if (instruction.Operand is not List<Instruction> bodyBytecode)
                {
                    diagnostics.Error(DiagnosticCode.HDEV,
                        "Invalid PROC op-code: Provided operand is not the function body's bytecode (List<Instruction>)",
                        instruction.Root?.GetFirstToken());
                    
                    break;
                }
                
                var bodyOptimizer = new BytecodeOptimizer(bodyBytecode, diagnostics);
                return instruction.WithOperand(bodyOptimizer.Optimize());
            }
        }
        
        return null;
    }

    private Instruction? PeekBytecode(int offset = 0) =>
        bytecode.ElementAtOrDefault(_pointer + offset);

    private void Advance(int amount = 1) => _pointer += amount;
}