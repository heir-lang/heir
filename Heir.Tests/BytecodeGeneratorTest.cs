using Heir.CodeGeneration;
using static Heir.Tests.Common;

namespace Heir.Tests
{
    public class BytecodeGeneratorTest
    {
        [Fact]
        public void GeneratesExit()
        {
            var bytecode = GenerateBytecode("69");
            var instruction = bytecode.Instructions.Last();
            Assert.Equal(OpCode.EXIT, instruction.OpCode);
            Assert.Null(instruction.Operand);
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
        [InlineData("'a' + 'b'", 'a', 'b', OpCode.CONCAT)]
        [InlineData("true && false", true, false, OpCode.AND)]
        public void Generates_BinaryOperations(string input, object? leftValue, object? rightValue, OpCode opCode)
        {
            var bytecode = GenerateBytecode(input);
            var pushLeft = bytecode.Instructions[0];
            var pushRight = bytecode.Instructions[1];
            var operation = bytecode.Instructions[2];
            Assert.Equal(OpCode.PUSH, pushLeft.OpCode);
            Assert.Equal(leftValue, pushLeft.Operand);
            Assert.Equal(OpCode.PUSH, pushRight.OpCode);
            Assert.Equal(rightValue, pushRight.Operand);
            Assert.Equal(opCode, operation.OpCode);
            Assert.Null(operation.Operand);
        }

        [Fact]
        public void Generates_Assignment()
        {
            var bytecode = GenerateBytecode("let mut a = 1; a = 2;").Skip(3);
            var loadIdentifier = bytecode.Instructions[0];
            var pushRight = bytecode.Instructions[1];
            var store = bytecode.Instructions[2];
        }

        [Theory]
        [InlineData("let mut a = 1; a += 1", 1L, OpCode.ADD)]
        [InlineData("let mut a = 1; a //= 2", 2L, OpCode.IDIV)]
        public void Generates_BinaryCompoundAssignment(string input, object? right, OpCode opCode)
        {
            var bytecode = GenerateBytecode(input).Skip(3);
            var pushIdentifier = bytecode.Instructions[0];
            var pushIdentifierAgain = bytecode.Instructions[1];
            var load = bytecode.Instructions[2];
            var pushRight = bytecode.Instructions[3];
            var operation = bytecode.Instructions[4];
            var store = bytecode.Instructions[5];
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
            Assert.Null(store.Operand);
        }

        [Theory]
        [InlineData("!false", false, OpCode.NOT)]
        [InlineData("~3", 3L, OpCode.BNOT)]
        [InlineData("-6", 6L, OpCode.UNM)]
        public void Generates_UnaryOperations(string input, object? operandValue, OpCode opCode)
        {
            var bytecode = GenerateBytecode(input);
            var push = bytecode.Instructions[0];
            var operation = bytecode.Instructions[1];
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
            var pushIdentifier = bytecode.Instructions[0];
            var pushIdentifierAgain = bytecode.Instructions[1];
            var load = bytecode.Instructions[2];
            var pushOne = bytecode.Instructions[3];
            var operation = bytecode.Instructions[4];
            var store = bytecode.Instructions[5];
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
            Assert.Null(store.Operand);
        }

        [Theory]
        [InlineData("let a = 1;", "a", 1L, OpCode.STORE)]
        [InlineData("let mut b = 2;", "b", 2L, OpCode.STOREMUTABLE)]
        public void Generates_VariableDeclarations(string input, string name, object? value, OpCode opCode)
        {
            var bytecode = GenerateBytecode(input);
            var pushIdentifier = bytecode.Instructions[0];
            var pushValue = bytecode.Instructions[1];
            var operation = bytecode.Instructions[2];
            Assert.Equal(OpCode.PUSH, pushIdentifier.OpCode);
            Assert.Equal(name, pushIdentifier.Operand);
            Assert.Equal(OpCode.PUSH, pushValue.OpCode);
            Assert.Equal(value, pushValue.Operand);
            Assert.Equal(opCode, operation.OpCode);
            Assert.Null(operation.Operand);
        }
    }
}
