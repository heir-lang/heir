using Heir.BoundAST;
using Heir.Types;
using static Heir.Tests.Common;

namespace Heir.Tests
{
    public class BinderTest
    {
        [Theory]
        [InlineData("\"abc\"", PrimitiveTypeKind.String)]
        [InlineData("'a'", PrimitiveTypeKind.Char)]
        [InlineData("123", PrimitiveTypeKind.Int)]
        [InlineData("69", PrimitiveTypeKind.Int)]
        [InlineData("0b1101", PrimitiveTypeKind.Int)]
        [InlineData("0o420", PrimitiveTypeKind.Int)]
        [InlineData("0x03E", PrimitiveTypeKind.Int)]
        [InlineData("123.456", PrimitiveTypeKind.Float)]
        [InlineData("69.420", PrimitiveTypeKind.Float)]
        [InlineData("true", PrimitiveTypeKind.Bool)]
        [InlineData("false", PrimitiveTypeKind.Bool)]
        [InlineData("none", PrimitiveTypeKind.None)]
        public void Binds_Literals(string input, PrimitiveTypeKind primitiveTypeKind)
        {
            var boundTree = Bind(input);
            var node = boundTree.Statements.First();
            Assert.IsType<BoundLiteral>(node);

            var literal = (BoundLiteral)node;
            Assert.Equal(input, literal.Token.Text);
            Assert.Equal(primitiveTypeKind, literal.Type.PrimitiveKind);
        }

        [Theory]
        [InlineData("true && false", PrimitiveTypeKind.Bool)]
        [InlineData("8 // 3", PrimitiveTypeKind.Int)]
        [InlineData("\"a\" + \"b\"", PrimitiveTypeKind.String)]
        [InlineData("'a' + 'b'", PrimitiveTypeKind.Char)]
        public void Binds_PrimitiveBinaryOperations(string input, PrimitiveTypeKind returnTypeKind)
        {
            var boundTree = Bind(input);
            var node = boundTree.Statements.First();
            Assert.IsType<BoundBinaryOp>(node);

            var binaryOp = (BoundBinaryOp)node;
            Assert.IsType<PrimitiveType>(binaryOp.Left.Type);
            Assert.IsType<PrimitiveType>(binaryOp.Right.Type);
            Assert.IsType<PrimitiveType>(binaryOp.Type);

            var returnType = (PrimitiveType)binaryOp.Type;
            Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
        }

        [Theory]
        [InlineData("7 / 4")]
        public void Binds_UnionBinaryOperations(string input)
        {
            var boundTree = Bind(input);
            var node = boundTree.Statements.First();
            Assert.IsType<BoundBinaryOp>(node);

            var binaryOp = (BoundBinaryOp)node;
            Assert.IsType<PrimitiveType>(binaryOp.Left.Type);
            Assert.IsType<PrimitiveType>(binaryOp.Right.Type);
            Assert.IsType<UnionType>(binaryOp.Type);

            var returnType = (UnionType)binaryOp.Type;
            var intType = returnType.Types.First();
            var floatType = returnType.Types.Last();
            Assert.IsType<PrimitiveType>(intType);
            Assert.IsType<PrimitiveType>(floatType);
            Assert.Equal(PrimitiveTypeKind.Int, ((PrimitiveType)intType).PrimitiveKind);
            Assert.Equal(PrimitiveTypeKind.Float, ((PrimitiveType)floatType).PrimitiveKind);
            Assert.Equal(TypeKind.Union, returnType.Kind);
        }
    }
}
