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
            
            if (optimizedInstruction != null)
            {
                optimizedBytecode.Add(optimizedInstruction);
                continue;
            }
            
            optimizedBytecode.Add(instruction);
            Advance();
        }

        return optimizedBytecode;
    }

    private Instruction RecursiveOptimize(Instruction instruction)
    {
        while (true)
        {
            var optimizedInstruction = Optimize(instruction);
            if (optimizedInstruction == null)
                return instruction;
            
            instruction = optimizedInstruction;
        }
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
                        var doubleResult = operation.OpCode == OpCode.UNM
                            ? -Convert.ToDouble(instruction.Operand)
                            : ~Convert.ToInt64(instruction.Operand);
                        
                        Advance();
                        var newInstruction = RecursiveOptimize(operation.OpCode == OpCode.UNM
                            ? instruction.WithOperand(doubleResult)
                            : instruction.WithOperand(Convert.ToInt64(doubleResult)));
                        
                        Advance();
                        return newInstruction;
                    }
                }
                if (PeekBytecode(1) is { OpCode: OpCode.NOT })
                {
                    var operand = Convert.ToBoolean(instruction.Operand);
                    var result = !operand;

                    Advance();
                    var newInstruction = RecursiveOptimize(instruction.WithOperand(result));
                    Advance();
                    return newInstruction;
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
                        
                        Advance(2);
                        var newInstruction = RecursiveOptimize(instruction.WithOperand(left + right));
                        Advance();

                        return newInstruction;
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

                        Advance(2);
                        var newInstruction = RecursiveOptimize(instruction.WithOperand(result));
                        Advance();

                        return newInstruction;
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

                        Advance(2);
                        var newInstruction = RecursiveOptimize(instruction.WithOperand(result));
                        Advance();

                        return newInstruction;
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

                        Advance(2);
                        var newInstruction = RecursiveOptimize(instruction.WithOperand(result));
                        Advance();

                        return newInstruction;
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

                        Advance(2);
                        var newInstruction = RecursiveOptimize(instruction.WithOperand(result));
                        Advance();

                        return newInstruction;
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

                        Advance(2);
                        var newInstruction = RecursiveOptimize(instruction.WithOperand(result));
                        Advance();

                        return newInstruction;
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