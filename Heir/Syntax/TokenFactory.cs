namespace Heir.Syntax;

public static class TokenFactory
{
    public static Token Keyword(SyntaxKind kind) => 
        new(kind, SyntaxFacts.KeywordMap.GetKey(kind), null, Location.Empty, Location.Empty);
        
    public static Token Keyword(SyntaxKind kind, Token token) => 
        new(kind, SyntaxFacts.KeywordMap.GetKey(kind), null, token.Span.Start, token.Span.End);
        
    public static Token Keyword(SyntaxKind kind, Location startLocation, Location endLocation) => 
        new(kind, SyntaxFacts.KeywordMap.GetKey(kind), null, startLocation, endLocation);

    public static Token Identifier(string text) => Identifier(text, Location.Empty, Location.Empty);
    public static Token Identifier(string text, Location startLocation, Location endLocation) =>
        new(SyntaxKind.Identifier, text, null, startLocation, endLocation);
        
    public static Token StringFromIdentifier(Token identifier) =>
        StringLiteral($"\"{identifier.Text}\"", identifier.Span.Start, identifier.Span.End);

    public static Token Operator(SyntaxKind kind, string text, Location startLocation, Location endLocation) =>
        new(kind, text, null, startLocation, endLocation);

    public static Token BoolLiteral(string text, Location startLocation, Location endLocation) =>
        new(SyntaxKind.BoolLiteral, text, text == "true", startLocation, endLocation);

    public static Token StringLiteral(string text, Location startLocation, Location endLocation) =>
        new(SyntaxKind.StringLiteral, text, text.Substring(1, text.Length - 2), startLocation, endLocation);

    public static Token CharLiteral(string text, Location startLocation, Location endLocation) =>
        new(SyntaxKind.CharLiteral, text, Convert.ToChar(text.Substring(1, text.Length - 2)), startLocation, endLocation);

    public static Token IntLiteral(string text, Location startLocation, Location endLocation, int radix = 10) =>
        new(SyntaxKind.IntLiteral, text, Convert.ToInt32(radix == 10 ? text : text[2..], radix), startLocation, endLocation);
    
    public static Token IntLiteral(int value, Location startLocation, Location endLocation) =>
        new(SyntaxKind.IntLiteral, value.ToString(), value, startLocation, endLocation);

    public static Token IntLiteral(int value, Token token) =>
        IntLiteral(value, token.Span.Start, token.Span.End);
    
    public static Token FloatLiteral(string text, Location startLocation, Location endLocation) =>
        new(SyntaxKind.FloatLiteral, text, Convert.ToDouble(text), startLocation, endLocation);

    public static Token NoneLiteral() => NoneLiteral(Location.Empty, Location.Empty);
    public static Token NoneLiteral(Token token) => NoneLiteral(token.Span.Start, token.Span.End);
    public static Token NoneLiteral(Location startLocation, Location endLocation) =>
        new(SyntaxKind.NoneKeyword, "none", null, startLocation, endLocation);

    public static TriviaToken Trivia(TriviaKind kind, string text, Location startLocation, Location endLocation) =>
        new(kind, text, startLocation, endLocation);
}