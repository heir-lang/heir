using Heir.AST;
using Heir.Syntax;

namespace Heir.Tests
{
    public class ParserTest
    {
        [Theory]
        [InlineData("1 + 2", SyntaxKind.Plus)]
        [InlineData("3 - 2", SyntaxKind.Minus)]
        [InlineData("3 * 4", SyntaxKind.Star)]
        [InlineData("6 % 4", SyntaxKind.Percent)]
        [InlineData("10 ^ 2", SyntaxKind.Carat)]
        [InlineData("2 & 7", SyntaxKind.Ampersand)]
        [InlineData("9 | 4", SyntaxKind.Pipe)]
        [InlineData("5 ~ 3", SyntaxKind.Tilde)]
        [InlineData("true && false", SyntaxKind.Tilde)]
        [InlineData("true || false", SyntaxKind.Tilde)]
        public void Parses_BinaryOperators(string input, SyntaxKind operatorKind)
        {
            var node = Parse(input);
            Assert.IsType<BinaryOp>(node);

            var binaryOperation = (BinaryOp)node;
            Assert.IsType<Literal>(binaryOperation.Left);
            Assert.IsType<Literal>(binaryOperation.Right);
            Assert.Equal(operatorKind, binaryOperation.Operator.Kind);
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
            var node = Parse(input);
            Assert.IsType<Literal>(node);

            var literal = (Literal)node;
            Assert.Equal(input, literal.Token.Text);
        }

        [Fact]
        public void Parses_ParenthesizedExpressions()
        {
            var node = Parse("(1 + 2)");
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
            var node = Parse(input);
            Assert.IsType<IdentifierName>(node);

            var identifier = (IdentifierName)node;
            Assert.Equal(input, identifier.Token.Text);
        }

        private SyntaxNode Parse(string input)
        {
            var lexer = new Lexer(input, "<testing>");
            var tokenStream = lexer.GetTokens();
            var parser = new Parser(tokenStream);
            return parser.Parse();
        }
    }
}
