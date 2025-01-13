using Heir.CodeGeneration;
using Heir.Runtime.Values;
using static Heir.Tests.Common;

namespace Heir.Tests;
using ObjectBytecode = Dictionary<List<Instruction>, List<Instruction>>;

public class BytecodeGeneratorTest
{
    [Fact]
    public void Generates_Exit()
    {
        var bytecode = GenerateBytecode("69");
        var instruction = bytecode.Instructions.Last();
        Assert.Equal(OpCode.EXIT, instruction.OpCode);
        Assert.Null(instruction.Operand);
    }
    
    [Fact]
    public void Generates_Return()
    {
        var bytecode = GenerateBytecode("return 123;");
        var instruction = bytecode.Instructions.Last();
        Assert.Equal(OpCode.RETURN, instruction.OpCode);
        Assert.Null(instruction.Operand);
    }

    [Fact]
    public void GeneratesPushObject_ForObjectLiterals()
    {
        var bytecode = GenerateBytecode("{ a: true }");
        var pushObject = bytecode.Instructions.First();
        Assert.Equal(OpCode.PUSHOBJECT, pushObject.OpCode);
        Assert.IsType<ObjectBytecode>(pushObject.Operand);

        var objectBytecode = (ObjectBytecode)pushObject.Operand;
        var keyBytecode = objectBytecode.Keys.First();
        var valueBytecode = objectBytecode.Values.First();
        var pushA = keyBytecode.First();
        var pushTrue = valueBytecode.First();
        Assert.Equal("a", pushA.Operand);
        Assert.Equal(true, pushTrue.Operand);
    }

    [Fact]
    public void GeneratesPush_ForLiterals()
    {
        {
            var bytecode = GenerateBytecode("69");
            var instruction = bytecode.Instructions.First();
            Assert.Equal(OpCode.PUSH, instruction.OpCode);
            Assert.Equal(69L, instruction.Operand);
        }
        {
            var bytecode = GenerateBytecode("69.420");
            var instruction = bytecode.Instructions.First();
            Assert.Equal(OpCode.PUSH, instruction.OpCode);
            Assert.Equal(69.420, instruction.Operand);
        }
        {
            var bytecode = GenerateBytecode("none");
            var instruction = bytecode.Instructions.First();
            Assert.Equal(OpCode.PUSHNONE, instruction.OpCode);
            Assert.Null(instruction.Operand);
        }
    }

    [Theory]
    [InlineData("1 + 2", 1L, 2L, OpCode.ADD)]
    [InlineData("7 // 3", 7L, 3L, OpCode.IDIV)]
    [InlineData("1 < 2", 1L, 2L, OpCode.LT)]
    [InlineData("1 <= 2", 1L, 2L, OpCode.LTE)]
    [InlineData("2 > 1", 2L, 1L, OpCode.LTE)]
    [InlineData("'a' == 'b'", 'a', 'b', OpCode.EQ)]
    [InlineData("'a' != 'b'", 'a', 'b', OpCode.EQ)]
    [InlineData("'a' + 'b'", 'a', 'b', OpCode.CONCAT)]
    [InlineData("true && false", true, false, OpCode.AND)]
    [InlineData("14 << 1", 14L, 1L, OpCode.BSHL)]
    public void Generates_BinaryOperations(string input, object? leftValue, object? rightValue, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input);
        var pushLeft = bytecode[0];
        var pushRight = bytecode[1];
        var operation = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushLeft.OpCode);
        Assert.Equal(leftValue, pushLeft.Operand);
        Assert.Equal(OpCode.PUSH, pushRight.OpCode);
        Assert.Equal(rightValue, pushRight.Operand);
        Assert.Equal(opCode, operation.OpCode);
        Assert.Null(operation.Operand);
    }

    [Theory]
    [InlineData("2 > 1", 2L, 1L, OpCode.LTE)]
    [InlineData("2 >= 1", 2L, 1L, OpCode.LT)]
    [InlineData("'a' != 'b'", 'a', 'b', OpCode.EQ)]
    public void Generates_InvertedBinaryOperations(string input, object? leftValue, object? rightValue, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input);
        var pushLeft = bytecode[0];
        var pushRight = bytecode[1];
        var operation = bytecode[2];
        var inversion = bytecode[3];
        Assert.Equal(OpCode.PUSH, pushLeft.OpCode);
        Assert.Equal(leftValue, pushLeft.Operand);
        Assert.Equal(OpCode.PUSH, pushRight.OpCode);
        Assert.Equal(rightValue, pushRight.Operand);
        Assert.Equal(opCode, operation.OpCode);
        Assert.Null(operation.Operand);
        Assert.Equal(OpCode.NOT, inversion.OpCode);
        Assert.Null(inversion.Operand);
    }

    [Fact]
    public void Generates_Assignment()
    {
        var bytecode = GenerateBytecode("let mut a = 1; a = 2;").Skip(3);
        var pushIdentifier = bytecode[0];
        var pushRight = bytecode[1];
        var store = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal("a", pushIdentifier.Operand);
        Assert.Equal(OpCode.PUSH, pushRight.OpCode);
        Assert.Equal(2L, pushRight.Operand);
        Assert.Equal(OpCode.STORE, store.OpCode);
        Assert.True(store.Operand as bool?);
    }

    [Theory]
    [InlineData("let mut a = 1; a += 1", 1L, OpCode.ADD)]
    [InlineData("let mut a = 1; a //= 2", 2L, OpCode.IDIV)]
    public void Generates_BinaryCompoundAssignment(string input, object? right, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input).Skip(3);
        var pushIdentifier = bytecode[0];
        var pushIdentifierAgain = bytecode[1];
        var load = bytecode[2];
        var pushRight = bytecode[3];
        var operation = bytecode[4];
        var store = bytecode[5];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal("a", pushIdentifier.Operand);
        Assert.Equal(OpCode.PUSH, pushIdentifierAgain.OpCode);
        Assert.Equal("a", pushIdentifierAgain.Operand);
        Assert.Equal(OpCode.LOAD, load.OpCode);
        Assert.Null(load.Operand);
        Assert.Equal(OpCode.PUSH, pushRight.OpCode);
        Assert.Equal(right, pushRight.Operand);
        Assert.Equal(opCode, operation.OpCode);
        Assert.Null(operation.Operand);
        Assert.Equal(OpCode.STORE, store.OpCode);
        Assert.True(store.Operand as bool?);
    }

    [Theory]
    [InlineData("!false", false, OpCode.NOT)]
    [InlineData("~3", 3L, OpCode.BNOT)]
    [InlineData("-6", 6L, OpCode.UNM)]
    public void Generates_UnaryOperations(string input, object? operandValue, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input);
        var push = bytecode[0];
        var operation = bytecode[1];
        Assert.Equal(OpCode.PUSH, push.OpCode);
        Assert.Equal(operandValue, push.Operand);
        Assert.Equal(opCode, operation.OpCode);
        Assert.Null(operation.Operand);
    }

    [Theory]
    [InlineData("let mut a = 1; ++a", OpCode.ADD)]
    [InlineData("let mut a = 1; --a", OpCode.SUB)]
    public void Generates_UnaryCompoundAssignment(string input, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input).Skip(3);
        var pushIdentifier = bytecode[0];
        var pushIdentifierAgain = bytecode[1];
        var load = bytecode[2];
        var pushOne = bytecode[3];
        var operation = bytecode[4];
        var store = bytecode[5];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal("a", pushIdentifier.Operand);
        Assert.Equal(OpCode.PUSH, pushIdentifierAgain.OpCode);
        Assert.Equal("a", pushIdentifierAgain.Operand);
        Assert.Equal(OpCode.LOAD, load.OpCode);
        Assert.Null(load.Operand);
        Assert.Equal(OpCode.PUSH, pushOne.OpCode);
        Assert.Equal(1, pushOne.Operand);
        Assert.Equal(opCode, operation.OpCode);
        Assert.Null(operation.Operand);
        Assert.Equal(OpCode.STORE, store.OpCode);
        Assert.True(store.Operand as bool?);
    }

    [Theory]
    [InlineData("let a = 1;", "a", 1L, OpCode.STORE)]
    [InlineData("let mut b = 2;", "b", 2L, OpCode.STORE)]
    public void Generates_VariableDeclarations(string input, string name, object? value, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input);
        var pushIdentifier = bytecode[0];
        var pushValue = bytecode[1];
        var operation = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal(name, pushIdentifier.Operand);
        Assert.Equal(OpCode.PUSH, pushValue.OpCode);
        Assert.Equal(value, pushValue.Operand);
        Assert.Equal(opCode, operation.OpCode);
        Assert.False(operation.Operand as bool?);
    }
    
    [Theory]
    [InlineData("fn abc -> 420;")]
    [InlineData("fn abc(x: int): int -> 123 + x;")]
    public void Generates_FunctionDeclarations(string input)
    {
        var bytecode = GenerateBytecode(input);
        var pushIdentifier = bytecode[0];
        var pushValue = bytecode[1];
        var operation = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal("abc", pushIdentifier.Operand);
        Assert.Equal(OpCode.PUSH, pushValue.OpCode);
        Assert.IsType<Function>(pushValue.Operand);
        Assert.Equal(OpCode.STORE, operation.OpCode);
        Assert.False(operation.Operand as bool?);
    }

    [Fact]
    public void Generates_Invocation()
    {
        var bytecode = GenerateBytecode("fn abc(x: int): int -> 123 + x; abc(69);").Skip(3);
        var pushIdentifier = bytecode[0];
        var load = bytecode[1];
        var call = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal("abc", pushIdentifier.Operand);
        Assert.Equal(OpCode.LOAD, load.OpCode);
        Assert.Null(load.Operand);
        Assert.Equal(OpCode.CALL, call.OpCode);
        Assert.IsType<List<List<Instruction>>>(call.Operand);
        
        var argumentsBytecode = (List<List<Instruction>>)call.Operand;
        Assert.Single(argumentsBytecode);
        
        var argumentBytecode = argumentsBytecode.First();
        var pushValue = argumentBytecode[0];
        Assert.Equal(OpCode.PUSH, pushValue.OpCode);
        Assert.Equal(69L, pushValue.Operand);
    }
}
