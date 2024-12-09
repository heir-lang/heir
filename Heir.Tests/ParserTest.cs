using Heir.AST;

namespace Heir.Tests
{
    public class ParserTest
    {
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

        private SyntaxNode Parse(string input)
        {
            var lexer = new Lexer(input, "<testing>");
            var tokenStream = lexer.GetTokens();
            var parser = new Parser(tokenStream);
            return parser.Parse();
        }
    }
}
