using Heir.Syntax;

namespace Heir.Tests
{
    public class LexerTest
    {
        [Theory]
        [InlineData("123", 123)]
        [InlineData("69", 69)]
        [InlineData("0b1101", 13)]
        [InlineData("0o69", 6)]
        [InlineData("0x03E", 62)]
        public void Tokenizes_IntLiterals(string input, int value)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.IntLiteral));
            Assert.Equal(literalToken.Text, input);
            Assert.Equal(literalToken.Value, value);
        }

        [Theory]
        [InlineData("123.456", 123.456)]
        [InlineData("69.420", 69.420)]
        public void Tokenizes_FloatLiterals(string input, double value)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.FloatLiteral));
            Assert.Equal(literalToken.Text, input);
            Assert.Equal(literalToken.Value, value);
        }

        private TokenStream Tokenize(string input, bool noTrivia = false)
        {
            var lexer = new Lexer(input, "<testing>");
            return lexer.GetTokens(noTrivia);
        }
    }
}