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
    }
}
