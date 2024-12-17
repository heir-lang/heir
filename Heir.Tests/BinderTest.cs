using Heir.BoundAST;
using Heir.Types;
using static Heir.Tests.Common;

namespace Heir.Tests
{
    public class BinderTest
    {
        [Theory]
        [InlineData("\"a\" + 1", "H010")]
        [InlineData("true * false", "H010")]
        public void ThrowsWith(string input, string expectedErrorCode)
        {
            var boundTree = Bind(input);
            Assert.True(boundTree.Diagnostics.HasErrors());
            Assert.Contains(boundTree.Diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
        }

        [Fact]
        public void Binds_VariableDeclarations()
        {
            var boundTree = Bind("let x: string;");
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundVariableDeclaration>(statement);

            var declaration = (BoundVariableDeclaration)statement;
            Assert.False(declaration.IsMutable);
            Assert.Null(declaration.Initializer);
            Assert.NotNull(declaration.Type);
            Assert.IsType<PrimitiveType>(declaration.Type);

            var type = (PrimitiveType)declaration.Type;
            Assert.Equal("string", type.Name);
        }

        [Fact]
        public void Infers_VariableDeclarationTypes()
        {
            var boundTree = Bind("let mut x = 1;");
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundVariableDeclaration>(statement);

            var declaration = (BoundVariableDeclaration)statement;
            Assert.True(declaration.IsMutable);
            Assert.NotNull(declaration.Initializer);
            Assert.IsType<BoundLiteral>(declaration.Initializer);

            Assert.NotNull(declaration.Type);
            Assert.IsType<PrimitiveType>(declaration.Type);

            var type = (PrimitiveType)declaration.Type;
            Assert.Equal("int", type.Name);
        }

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
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundExpressionStatement>(statement);

            var node = ((BoundExpressionStatement)statement).Expression;
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
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundExpressionStatement>(statement);

            var node = ((BoundExpressionStatement)statement).Expression;
            Assert.IsType<BoundBinaryOp>(node);

            var binaryOp = (BoundBinaryOp)node;
            Assert.IsType<PrimitiveType>(binaryOp.Left.Type);
            Assert.IsType<PrimitiveType>(binaryOp.Right.Type);
            Assert.IsType<PrimitiveType>(binaryOp.Type);

            var returnType = (PrimitiveType)binaryOp.Type;
            Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
        }

        [Theory]
        [InlineData("7 / 4", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)]
        [InlineData("\"a\" + 'b'", PrimitiveTypeKind.String, PrimitiveTypeKind.Char)]
        public void Binds_UnionBinaryOperations(string input, PrimitiveTypeKind typeAKind, PrimitiveTypeKind typeBKind)
        {
            var boundTree = Bind(input);
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundExpressionStatement>(statement);

            var node = ((BoundExpressionStatement)statement).Expression;
            Assert.IsType<BoundBinaryOp>(node);

            var binaryOp = (BoundBinaryOp)node;
            Assert.IsType<PrimitiveType>(binaryOp.Left.Type);
            Assert.IsType<PrimitiveType>(binaryOp.Right.Type);
            Assert.IsType<UnionType>(binaryOp.Type);

            var returnType = (UnionType)binaryOp.Type;
            var typeA = returnType.Types.First();
            var typeB = returnType.Types.Last();
            Assert.IsType<PrimitiveType>(typeA);
            Assert.IsType<PrimitiveType>(typeB);
            Assert.Equal(typeAKind, ((PrimitiveType)typeA).PrimitiveKind);
            Assert.Equal(typeBKind, ((PrimitiveType)typeB).PrimitiveKind);
            Assert.Equal(TypeKind.Union, returnType.Kind);
        }

        [Theory]
        [InlineData("!false", PrimitiveTypeKind.Bool)]
        [InlineData("~6", PrimitiveTypeKind.Int)]
        public void Binds_PrimitiveUnaryOperations(string input, PrimitiveTypeKind returnTypeKind)
        {
            var boundTree = Bind(input);
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundExpressionStatement>(statement);

            var node = ((BoundExpressionStatement)statement).Expression;
            Assert.IsType<BoundUnaryOp>(node);

            var unaryOp = (BoundUnaryOp)node;
            Assert.IsType<PrimitiveType>(unaryOp.Operand.Type);
            Assert.IsType<PrimitiveType>(unaryOp.Type);

            var returnType = (PrimitiveType)unaryOp.Type;
            Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
        }

        [Theory]
        [InlineData("-69.420", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)]
        [InlineData("++6", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)] // invalid but only for testing purposes so idc
        public void Binds_UnionUnaryOperations(string input, PrimitiveTypeKind typeAKind, PrimitiveTypeKind typeBKind)
        {
            var boundTree = Bind(input);
            var statement = boundTree.Statements.First();
            Assert.IsType<BoundExpressionStatement>(statement);

            var node = ((BoundExpressionStatement)statement).Expression;
            Assert.IsType<BoundUnaryOp>(node);

            var unaryOp = (BoundUnaryOp)node;
            Assert.IsType<PrimitiveType>(unaryOp.Operand.Type);
            Assert.IsType<UnionType>(unaryOp.Type);

            var returnType = (UnionType)unaryOp.Type;
            var typeA = returnType.Types.First();
            var typeB = returnType.Types.Last();
            Assert.IsType<PrimitiveType>(typeA);
            Assert.IsType<PrimitiveType>(typeB);
            Assert.Equal(typeAKind, ((PrimitiveType)typeA).PrimitiveKind);
            Assert.Equal(typeBKind, ((PrimitiveType)typeB).PrimitiveKind);
            Assert.Equal(TypeKind.Union, returnType.Kind);
        }
    }
}
