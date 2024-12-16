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
    }
}
