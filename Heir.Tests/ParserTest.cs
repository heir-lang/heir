using Heir.AST;
using Heir.Syntax;

namespace Heir.Tests
{
    public class ParserTest
    {
        [Theory]
        [InlineData("1 += 2", "H008")]
        [InlineData("()", "H007")]
        [InlineData("++1", "H006")]
        [InlineData("--3", "H006")]
        [InlineData("]", "H005")]
        public void ThrowsWith(string input, string expectedErrorCode)
        {
            var (_, diagnostics) = Parse(input);
            Assert.True(diagnostics.HasErrors());
            Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
        }

        [Theory]
        [InlineData("++a")]
        [InlineData("--b")]
        public void DoesNotThrowWith(string input)
        {
            var (_, diagnostics) = Parse(input);
            Assert.False(diagnostics.HasErrors());
        }

        [Theory]
        [InlineData("!true", SyntaxKind.Bang)]
        [InlineData("++a", SyntaxKind.PlusPlus)]
        [InlineData("--b", SyntaxKind.MinusMinus)]
        [InlineData("-10", SyntaxKind.Minus)]
        [InlineData("~14", SyntaxKind.Tilde)]
        public void Parses_UnaryOperators(string input, SyntaxKind operatorKind)
        {
            var (node, _) = Parse(input);
            Assert.IsType<UnaryOp>(node);

            var unaryOperation = (UnaryOp)node;
            Assert.Equal(operatorKind, unaryOperation.Operator.Kind);
        }

        [Theory]
        [InlineData("1 + 2", SyntaxKind.Plus)]
        [InlineData("3 - 2", SyntaxKind.Minus)]
        [InlineData("3 * 4", SyntaxKind.Star)]
        [InlineData("6 % 4", SyntaxKind.Percent)]
        [InlineData("10 ^ 2", SyntaxKind.Carat)]
        [InlineData("2 & 7", SyntaxKind.Ampersand)]
        [InlineData("9 | 4", SyntaxKind.Pipe)]
        [InlineData("5 ~ 3", SyntaxKind.Tilde)]
        [InlineData("true && false", SyntaxKind.AmpersandAmpersand)]
        [InlineData("true || false", SyntaxKind.PipePipe)]
        public void Parses_BinaryOperators(string input, SyntaxKind operatorKind)
        {
            var (node, _) = Parse(input);
            Assert.IsType<BinaryOp>(node);

            var binaryOperation = (BinaryOp)node;
            Assert.IsType<Literal>(binaryOperation.Left);
            Assert.IsType<Literal>(binaryOperation.Right);
            Assert.Equal(operatorKind, binaryOperation.Operator.Kind);
        }

        [Fact]
        public void Parses_OperatorPrecedence()
        {
            {
                var (node, _) = Parse("3 ^ 2 * 4 - 2");
                Assert.IsType<BinaryOp>(node);

                var subtraction = (BinaryOp)node;
                Assert.IsType<BinaryOp>(subtraction.Left);
                var multiplication = (BinaryOp)subtraction.Left;
                Assert.IsType<Literal>(subtraction.Right);
                var twoLiteral = (Literal)subtraction.Right;
                Assert.IsType<BinaryOp>(multiplication.Left);
                var exponentation = (BinaryOp)multiplication.Left;
                Assert.IsType<Literal>(multiplication.Right);
                var fourLiteral = (Literal)multiplication.Right;

                Assert.Equal(SyntaxKind.Minus, subtraction.Operator.Kind);
                Assert.Equal(SyntaxKind.Star, multiplication.Operator.Kind);
                Assert.Equal(SyntaxKind.Carat, exponentation.Operator.Kind);
                Assert.Equal((long)2, twoLiteral.Token.Value);
                Assert.Equal((long)4, fourLiteral.Token.Value);
            }
            {
                var (node, _) = Parse("true || false && true");
                Assert.IsType<BinaryOp>(node);

                var or = (BinaryOp)node;
                Assert.IsType<Literal>(or.Left);
                var trueLiteral = (Literal)or.Left;
                Assert.IsType<BinaryOp>(or.Right);
                var and = (BinaryOp)or.Right;
                Assert.IsType<Literal>(and.Left);
                Assert.IsType<Literal>(and.Right);

                Assert.Equal(SyntaxKind.PipePipe, or.Operator.Kind);
                Assert.Equal(SyntaxKind.AmpersandAmpersand, and.Operator.Kind);
                Assert.Equal(true, trueLiteral.Token.Value);
            }
            {
                var (node, _) = Parse("x += y * z");
                Assert.IsType<AssignmentOp>(node);

                var assignment = (AssignmentOp)node;
                Assert.IsType<IdentifierName>(assignment.Left);
                var target = (IdentifierName)assignment.Left;
                Assert.IsType<BinaryOp>(assignment.Right);
                var multiplication = (BinaryOp)assignment.Right;
                Assert.IsType<IdentifierName>(multiplication.Left);
                Assert.IsType<IdentifierName>(multiplication.Right);

                Assert.Equal(SyntaxKind.PlusEquals, assignment.Operator.Kind);
                Assert.Equal(SyntaxKind.Star, multiplication.Operator.Kind);
            }
        }

        [Theory]
        [InlineData("\"abc\"")]
        [InlineData("'a'")]
        [InlineData("123")]
        [InlineData("69")]
        [InlineData("0b1101")]
        [InlineData("0o420")]
        [InlineData("0x03E")]
        [InlineData("123.456")]
        [InlineData("69.420")]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("none")]
        public void Parses_Literals(string input)
        {
            var (node, _) = Parse(input);
            Assert.IsType<Literal>(node);

            var literal = (Literal)node;
            Assert.Equal(input, literal.Token.Text);
        }

        [Fact]
        public void Parses_ParenthesizedExpressions()
        {
            var (node, _) = Parse("(1 + 2)");
            Assert.IsType<Parenthesized>(node);

            var parenthesized = (Parenthesized)node;
            Assert.IsType<BinaryOp>(parenthesized.Expression);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("abc123")]
        [InlineData("abc_123")]
        public void Parses_Identifiers(string input)
        {
            var (node, _) = Parse(input);
            Assert.IsType<IdentifierName>(node);

            var identifier = (IdentifierName)node;
            Assert.Equal(input, identifier.Token.Text);
        }

        private (SyntaxNode, DiagnosticBag) Parse(string input)
        {
            var lexer = new Lexer(input, "<testing>");
            var tokenStream = lexer.GetTokens();
            var parser = new Parser(tokenStream);

            // temp
            return (parser.Parse(), parser.Diagnostics);
        }
    }
}
