using Heir.Syntax;

namespace Heir.Tests
{
    public class LexerTest
    {
        [Fact]
        public void ThrowsWith_MalformedNumber()
        {
            var tokenStream = Tokenize("1.2.3", noTrivia: true);
            Assert.True(tokenStream.Diagnostics.HasErrors());

            var error = tokenStream.Diagnostics.First();
            Assert.Equal("H004", error.Code);
        }

        [Fact]
        public void ThrowsWith_UnterminatedStringOrChar()
        {
            {
                var tokenStream = Tokenize("'c", noTrivia: true);
                Assert.True(tokenStream.Diagnostics.HasErrors());

                var error = tokenStream.Diagnostics.First();
                Assert.Equal("H002", error.Code);
            }
            {
                var tokenStream = Tokenize("\"ab", noTrivia: true);
                Assert.True(tokenStream.Diagnostics.HasErrors());

                var error = tokenStream.Diagnostics.First();
                Assert.Equal("H003", error.Code);
            }
        }

        [Fact]
        public void ThrowsWith_UnexpectedCharacter()
        {
            var tokenStream = Tokenize("\\", noTrivia: true);
            Assert.True(tokenStream.Diagnostics.HasErrors());

            var error = tokenStream.Diagnostics.First();
            Assert.Equal("H001", error.Code);
        }

        [Theory]
        [InlineData("\"abc\"", "abc")]
        public void Tokenizes_StringLiterals(string input, string value)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.StringLiteral));
            Assert.Equal(input, literalToken.Text);
            Assert.Equal(value, literalToken.Value);
        }

        [Theory]
        [InlineData("'a'", 'a')]
        public void Tokenizes_CharLiterals(string input, char value)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.CharLiteral));
            Assert.Equal(input, literalToken.Text);
            Assert.Equal(value, literalToken.Value);
        }

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
            Assert.Equal(input, literalToken.Text);
            Assert.Equal(value, literalToken.Value);
        }

        [Theory]
        [InlineData("123.456", 123.456)]
        [InlineData("69.420", 69.420)]
        public void Tokenizes_FloatLiterals(string input, double value)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.FloatLiteral));
            Assert.Equal(input, literalToken.Text);
            Assert.Equal(value, literalToken.Value);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void Tokenizes_BoolLiterals(string input, bool value)
        {
            var tokenStream = Tokenize(input, noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.BoolLiteral));
            Assert.Equal(input, literalToken.Text);
            Assert.Equal(value, literalToken.Value);
        }

        [Fact]
        public void Tokenizes_NoneLiterals()
        {
            var tokenStream = Tokenize("none", noTrivia: true);
            var literalToken = tokenStream.First();
            Assert.True(literalToken.IsKind(SyntaxKind.NoneLiteral));
            Assert.Equal("none", literalToken.Text);
            Assert.Null(literalToken.Value);
        }

        private TokenStream Tokenize(string input, bool noTrivia = false)
        {
            var lexer = new Lexer(input, "<testing>");
            return lexer.GetTokens(noTrivia);
        }
    }
}