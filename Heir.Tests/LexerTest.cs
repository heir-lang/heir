using Heir.Diagnostics;
using Heir.Syntax;

namespace Heir.Tests;

public class LexerTest
{
    [Theory]
    [InlineData("1.2.3", DiagnosticCode.H003)]
    [InlineData("'c", DiagnosticCode.H002B)]
    [InlineData("\"ab", DiagnosticCode.H002)]
    [InlineData(@"\\", DiagnosticCode.H001)]
    public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
    {
        var tokenStream = Tokenize(input);
        Assert.True(tokenStream.Diagnostics.HasErrors);
        Assert.Contains(tokenStream.Diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
    }

    [Theory]
    [InlineData("\"abc\"", SyntaxKind.StringLiteral, "abc")]
    [InlineData("'a'", SyntaxKind.CharLiteral, 'a')]
    [InlineData("123", SyntaxKind.IntLiteral, 123)]
    [InlineData("69", SyntaxKind.IntLiteral, 69)]
    [InlineData("0b1101", SyntaxKind.IntLiteral, 13)]
    [InlineData("0o420", SyntaxKind.IntLiteral, 272)]
    [InlineData("0x03E", SyntaxKind.IntLiteral, 62)]
    [InlineData("123.456", SyntaxKind.FloatLiteral, 123.456)]
    [InlineData("69.420", SyntaxKind.FloatLiteral, 69.420)]
    [InlineData("true", SyntaxKind.BoolLiteral, true)]
    [InlineData("false", SyntaxKind.BoolLiteral, false)]
    [InlineData("none", SyntaxKind.NoneKeyword, null)]
    public void Tokenizes_Literals(string input, SyntaxKind expectedKind, object expectedValue)
    {
        var tokenStream = Tokenize(input);
        var literalToken = tokenStream.First();

        Assert.Equal(expectedKind, literalToken.Kind);
        Assert.Equal(input, literalToken.Text);
        Assert.Equal(expectedValue, literalToken.Value);
    }

    [Fact]
    public void Tokenizes_Operators()
    {
        foreach (var (input, kind) in SyntaxFacts.OperatorMap.Forward)
        {
            var tokenStream = Tokenize(input);
            var token = tokenStream.First();

            Assert.Equal(kind, token.Kind);
            Assert.Equal(input, token.Text);
            Assert.Null(token.Value);
        }
    }

    [Theory]
    [InlineData(".", SyntaxKind.Dot)]
    [InlineData(":", SyntaxKind.Colon)]
    [InlineData("::", SyntaxKind.ColonColon)]
    [InlineData(",", SyntaxKind.Comma)]
    public void Tokenizes_MiscSymbols(string input, SyntaxKind expectedKind)
    {
        var tokenStream = Tokenize(input);
        var token = tokenStream.First();

        Assert.Equal(expectedKind, token.Kind);
        Assert.Equal(input, token.Text);
        Assert.Null(token.Value);
    }

    [Theory]
    [InlineData("let", SyntaxKind.LetKeyword)]
    [InlineData("mut", SyntaxKind.MutKeyword)]
    [InlineData("int", SyntaxKind.IntKeyword)]
    [InlineData("float", SyntaxKind.FloatKeyword)]
    [InlineData("string", SyntaxKind.StringKeyword)]
    [InlineData("char", SyntaxKind.CharKeyword)]
    [InlineData("bool", SyntaxKind.BoolKeyword)]
    [InlineData("none", SyntaxKind.NoneKeyword)]
    public void Tokenizes_Keywords(string input, SyntaxKind expectedKind)
    {
        var tokenStream = Tokenize(input);
        var token = tokenStream.First();

        Assert.Equal(expectedKind, token.Kind);
        Assert.Equal(input, token.Text);
        Assert.Null(token.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc123")]
    [InlineData("abc_123")]
    public void Tokenizes_Identifiers(string input)
    {
        var tokenStream = Tokenize(input);
        var token = tokenStream.First();

        Assert.Equal(SyntaxKind.Identifier, token.Kind);
        Assert.Equal(input, token.Text);
        Assert.Null(token.Value);
    }
    
    [Theory]
    [InlineData("## single line comment")]
    [InlineData("###\nmulti\nline\ncomment###")]
    [InlineData(";;;;;;;;")]
    [InlineData("   \r\n\r\n \n\n    \t\t\t   ")] // whitespaces

    public void Tokenizes_Trivia(string input)
    {
        var tokenStream = Tokenize(input);
        Assert.Empty(tokenStream);
        Assert.Empty(tokenStream.Diagnostics);
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("+")]
    [InlineData("\n\nabc", 3)]
    [InlineData("## abc", 1, null, null, false)]
    [InlineData("###\na\nb\nc\n###", 1, 5, 4, false)]
    public void Tokens_HaveCorrectSpans(string input, int startLine = 1, int? endLine = null, int? endColumn = null, bool withoutTrivia = true)
    {
        endLine ??= startLine;
        
        var tokenStream = Common.Tokenize(input);
        if (withoutTrivia)
            tokenStream = tokenStream.WithoutTrivia();
        
        Assert.NotEmpty(tokenStream);
        
        var token = tokenStream.First();
        const int startColumn = 0;
        endColumn ??= token.Text.Length;
        
        Assert.Equal(startColumn, token.Span.Start.Column);
        Assert.Equal(endColumn, token.Span.End.Column);
        Assert.Equal(startLine, token.Span.Start.Line);
        Assert.Equal(endLine, token.Span.End.Line);
    }

    private static TokenStream Tokenize(string input) => Common.Tokenize(input).WithoutTrivia();
}
