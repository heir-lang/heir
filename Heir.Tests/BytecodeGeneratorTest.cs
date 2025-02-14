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
        var instruction = bytecode[^1];
        Assert.Equal(OpCode.EXIT, instruction.OpCode);
        Assert.Null(instruction.Operand);
    }

    [Fact]
    public void Generates_EnumDeclarations()
    {
        const string input = """
                             enum Abc {
                                A,
                                B,
                                C
                             }
                             """;

        var bytecode = GenerateBytecode(input);
        var pushName = bytecode[0];
        var pushObject = bytecode[1];
        var store = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushName.OpCode);
        Assert.Equal("Abc", pushName.Operand);
        Assert.Equal(OpCode.PUSHOBJECT, pushObject.OpCode);
        Assert.IsType<ObjectBytecode>(pushObject.Operand);
        Assert.Equal(OpCode.STORE, store.OpCode);
        Assert.Equal(false, store.Operand);
        
        var objectBytecode = (ObjectBytecode)pushObject.Operand;
        Assert.Equal(3, objectBytecode.Count);
    }

    [Theory]
    [InlineData("abc[\"buh\"]")]
    [InlineData("abc.buh")]
    public void Generates_Index(string indexExpression)
    {
        var bytecode = GenerateBytecode("let abc = { buh: true }; " + indexExpression + ";").Skip(3);
        Assert.Equal(OpCode.LOAD, bytecode[0].OpCode);
        Assert.Equal("abc", bytecode[0].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[1].OpCode);
        Assert.Equal("buh", bytecode[1].Operand);
        Assert.Equal(OpCode.INDEX, bytecode[2].OpCode);
        Assert.Null(bytecode[2].Operand);
    }

    [Fact]
    public void Generates_WhileStatements()
    {
        const string input = """
                             while i < 10
                                ++i;
                             """;

        var bytecode = GenerateBytecode(input);
        Assert.Equal(OpCode.LOAD, bytecode[0].OpCode);
        Assert.Equal("i", bytecode[0].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[1].OpCode);
        Assert.Equal(10, bytecode[1].Operand);
        Assert.Equal(OpCode.LT, bytecode[2].OpCode);
        Assert.Null(bytecode[2].Operand);
        Assert.Equal(OpCode.JZ, bytecode[3].OpCode);
        Assert.Equal(3, bytecode[3].Operand);
        Assert.Equal(OpCode.INC, bytecode[4].OpCode);
        Assert.Equal("i", bytecode[4].Operand);
        Assert.Equal(OpCode.JMP, bytecode[5].OpCode);
        Assert.Equal(-5, bytecode[5].Operand);
    }

    [Fact]
    public void Generates_IfStatements()
    {
        const string input = """
                             if x == 1
                                 x + 1;
                             else if x == 2
                                 x + 2;
                             else
                                 x * 5;
                             """;
        
        var bytecode = GenerateBytecode(input);
        Assert.Equal(OpCode.LOAD, bytecode[0].OpCode);
        Assert.Equal("x", bytecode[0].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[1].OpCode);
        Assert.Equal(1, bytecode[1].Operand);
        Assert.Equal(OpCode.EQ, bytecode[2].OpCode);
        Assert.Equal(OpCode.JNZ, bytecode[3].OpCode);
        Assert.Equal(13, bytecode[3].Operand);
        Assert.Equal(OpCode.LOAD, bytecode[4].OpCode);
        Assert.Equal("x", bytecode[4].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[5].OpCode);
        Assert.Equal(2, bytecode[5].Operand);
        Assert.Equal(OpCode.EQ, bytecode[6].OpCode);
        Assert.Equal(OpCode.JNZ, bytecode[7].OpCode);
        Assert.Equal(5, bytecode[7].Operand);
        Assert.Equal(OpCode.LOAD, bytecode[8].OpCode);
        Assert.Equal("x", bytecode[8].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[9].OpCode);
        Assert.Equal(5, bytecode[9].Operand);
        Assert.Equal(OpCode.MUL, bytecode[10].OpCode);
        Assert.Equal(OpCode.JMP, bytecode[11].OpCode);
        Assert.Equal(4, bytecode[11].Operand);
        Assert.Equal(OpCode.LOAD, bytecode[12].OpCode);
        Assert.Equal("x", bytecode[12].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[13].OpCode);
        Assert.Equal(2, bytecode[13].Operand);
        Assert.Equal(OpCode.ADD, bytecode[14].OpCode);
        Assert.Equal(OpCode.JMP, bytecode[15].OpCode);
        Assert.Equal(4, bytecode[15].Operand);
        Assert.Equal(OpCode.LOAD, bytecode[16].OpCode);
        Assert.Equal("x", bytecode[16].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[17].OpCode);
        Assert.Equal(1, bytecode[17].Operand);
        Assert.Equal(OpCode.ADD, bytecode[18].OpCode);
    }
    
    [Fact]
    public void Generates_ReturnStatements()
    {
        var bytecode = GenerateBytecode("return 123;");
        var instruction = bytecode[^1];
        Assert.Equal(OpCode.RETURN, instruction.OpCode);
        Assert.Null(instruction.Operand);
    }

    [Fact]
    public void GeneratesPushObject_ForObjectLiterals()
    {
        var bytecode = GenerateBytecode("{ a: true }");
        var pushObject = bytecode[0];
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
    public void Generates_NothingForNullForgiving()
    {
        var bytecode = GenerateBytecode("1!");
        var instruction = bytecode[0];
        Assert.Equal(OpCode.PUSH, instruction.OpCode);
        Assert.Equal(1, instruction.Operand);
    }

    [Theory]
    [InlineData("let inline x = 1; x;", OpCode.PUSH, 1)]
    [InlineData("69", OpCode.PUSH, 69)]
    [InlineData("69.420", OpCode.PUSH, 69.420)]
    [InlineData("none", OpCode.PUSHNONE)]
    public void GeneratesPush_ForLiterals(string input, OpCode expectedOpCode, object? expectedOperand = null)
    {
        var bytecode = GenerateBytecode(input);
        var instruction = bytecode[0];
        Assert.Equal(expectedOpCode, instruction.OpCode);
        Assert.Equal(expectedOperand, instruction.Operand);
    }

    [Theory]
    [InlineData("1 + 2", 3.0)]
    [InlineData("7 // 3", 2)]
    [InlineData("1 > 2", false)]
    [InlineData("1 >= 2", false)]
    [InlineData("1 < 2", true)]
    [InlineData("1 <= 2", true)]
    [InlineData("2 <= 2", true)]
    [InlineData("2 > 1", true)]
    [InlineData("2 >= 1", true)]
    [InlineData("'a' == 'b'", false)]
    [InlineData("'a' != 'b'", true)]
    [InlineData("'a' + 'b'", "ab")]
    [InlineData("true && false", false)]
    [InlineData("14 << 1", 28)]
    public void Generates_BinaryOperations(string input, object? resultValue)
    {
        var bytecode = GenerateBytecode(input);
        var pushResult = bytecode[0];
        Assert.Equal(OpCode.PUSH, pushResult.OpCode);
        Assert.Equal(resultValue, pushResult.Operand);
    }

    [Fact]
    public void Generates_Assignments()
    {
        var bytecode = GenerateBytecode("let mut a = 1; a = 2;").Skip(3);
        var pushIdentifier = bytecode[0];
        var pushRight = bytecode[1];
        var store = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal("a", pushIdentifier.Operand);
        Assert.Equal(OpCode.PUSH, pushRight.OpCode);
        Assert.Equal(2, pushRight.Operand);
        Assert.Equal(OpCode.STORE, store.OpCode);
        Assert.True(store.Operand as bool?);
    }

    [Theory]
    [InlineData("let mut a = 1; a += 1", 1, OpCode.ADD)]
    [InlineData("let mut a = 1; a //= 2", 2, OpCode.IDIV)]
    public void Generates_BinaryCompoundAssignment(string input, object? right, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input).Skip(3);
        Assert.Equal(OpCode.PUSH, bytecode[0].OpCode);
        Assert.Equal("a", bytecode[0].Operand);
        Assert.Equal(OpCode.LOAD, bytecode[1].OpCode);
        Assert.Equal("a", bytecode[1].Operand);
        Assert.Equal(OpCode.PUSH, bytecode[2].OpCode);
        Assert.Equal(right, bytecode[2].Operand);
        Assert.Equal(opCode, bytecode[3].OpCode);
        Assert.Null(bytecode[3].Operand);
        Assert.Equal(OpCode.STORE, bytecode[4].OpCode);
        Assert.True(bytecode[4].Operand as bool?);
    }

    [Theory]
    [InlineData("!false", true)]
    [InlineData("!true", false)]
    [InlineData("!!true", true)]
    [InlineData("!!!true", false)]
    [InlineData("!!!!true", true)]
    [InlineData("~3", -4L)]
    [InlineData("-6.0", -6.0)]
    [InlineData("-6", -6)]
    [InlineData("-(-(-6))", -6)]
    public void Generates_UnaryOperations(string input, object? resultValue)
    {
        var bytecode = GenerateBytecode(input);
        var pushResult = bytecode[0];
        Assert.Equal(OpCode.PUSH, pushResult.OpCode);
        Assert.Equal(resultValue, pushResult.Operand);
    }

    [Theory]
    [InlineData("let mut a = 1; ++a", OpCode.INC)]
    [InlineData("let mut a = 1; --a", OpCode.DEC)]
    public void Generates_UnaryCompoundAssignments(string input, OpCode opCode)
    {
        var bytecode = GenerateBytecode(input).Skip(3);
        var operation = bytecode[0];
        Assert.Equal(opCode, operation.OpCode);
        Assert.Equal("a", operation.Operand);
    }

    [Theory]
    [InlineData("let a = 1;", "a", 1)]
    [InlineData("let mut b = 2;", "b", 2)]
    [InlineData("let c: int;", "c", null, OpCode.PUSHNONE)]
    public void Generates_VariableDeclarations(string input, string name, object? value, OpCode pushOpCode = OpCode.PUSH)
    {
        var bytecode = GenerateBytecode(input);
        var pushIdentifier = bytecode[0];
        var pushValue = bytecode[1];
        var operation = bytecode[2];
        Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
        Assert.Equal(name, pushIdentifier.Operand);
        Assert.Equal(pushOpCode, pushValue.OpCode);
        Assert.Equal(value, pushValue.Operand);
        Assert.Equal(OpCode.STORE, operation.OpCode);
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
        Assert.Equal(OpCode.PROC, pushValue.OpCode);
        Assert.IsType<AST.FunctionDeclaration>(pushValue.Root);
        Assert.IsType<List<Instruction>>(pushValue.Operand);
        Assert.Equal(OpCode.STORE, operation.OpCode);
        Assert.False(operation.Operand as bool?);
    }

    [Fact]
    public void Generates_Invocations()
    {
        var bytecode = GenerateBytecode("fn abc(x: int): int -> 123 + x; abc(69);").Skip(3);
        var load = bytecode[0];
        var call = bytecode[1];
        var pushXArgument = bytecode[2];
        Assert.Equal(OpCode.LOAD, load.OpCode);
        Assert.Equal("abc", load.Operand);
        Assert.Equal(OpCode.CALL, call.OpCode);
        Assert.IsType<ValueTuple<int, List<string>>>(call.Operand);
        
        var (argumentInstructionsCount, parameterNames) = (ValueTuple<int, List<string>>)call.Operand;
        Assert.Equal(1, argumentInstructionsCount);
        Assert.Single(parameterNames);
        
        Assert.Equal(OpCode.PUSH, pushXArgument.OpCode);
        Assert.Equal(69, pushXArgument.Operand);
    }
}
