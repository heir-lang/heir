using Heir.Syntax;

namespace Heir.Tests
{
    public class LexerTest
    {
        [Theory]
        [InlineData("1.2.3", "H004")]
        [InlineData("'c", "H002")]
        [InlineData("\"ab", "H003")]
        [InlineData(@"\\", "H001")]
        public void ThrowsWith_Error(string input, string expectedErrorCode)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            Assert.True(tokenStream.Diagnostics.HasErrors());

            var error = tokenStream.Diagnostics.First();
            Assert.Equal(expectedErrorCode, error.Code);
        }

        [Theory]
        [InlineData("\"abc\"", SyntaxKind.StringLiteral, "\"abc\"", "abc")]
        [InlineData("'a'", SyntaxKind.CharLiteral, "'a'", "a")]
        [InlineData("123", SyntaxKind.IntLiteral, "123", (long)123)]
        [InlineData("69", SyntaxKind.IntLiteral, "69", (long)69)]
        [InlineData("0b1101", SyntaxKind.IntLiteral, "0b1101", (long)13)]
        [InlineData("0o420", SyntaxKind.IntLiteral, "0o420", (long)272)]
        [InlineData("0x03E", SyntaxKind.IntLiteral, "0x03E", (long)62)]
        [InlineData("123.456", SyntaxKind.FloatLiteral, "123.456", 123.456)]
        [InlineData("69.420", SyntaxKind.FloatLiteral, "69.420", 69.420)]
        [InlineData("true", SyntaxKind.BoolLiteral, "true", true)]
        [InlineData("false", SyntaxKind.BoolLiteral, "false", false)]
        [InlineData("none", SyntaxKind.NoneLiteral, "none", null)]
        public void Tokenizes_Literals(string input, SyntaxKind expectedKind, string expectedText, object expectedValue)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();

            Assert.True(literalToken.IsKind(expectedKind));
            Assert.Equal(expectedText, literalToken.Text);
            Assert.Equal(expectedValue, literalToken.Value);
        }

        private TokenStream Tokenize(string input, bool noTrivia = false)
        {
            var lexer = new Lexer(input, "<testing>");
            return lexer.GetTokens(noTrivia);
        }
    }
}