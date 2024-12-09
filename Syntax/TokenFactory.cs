namespace Heir.Syntax
{
    public static class TokenFactory
    {
        public static Token Keyword(SyntaxKind kind, Location startLocation, Location endLocation)
        {
            return new Token(kind, SyntaxFacts.KeywordMap.GetKey(kind), null, startLocation, endLocation);
        }

        public static Token Identifier(string text, Location startLocation, Location endLocation)
        {
            return new Token(SyntaxKind.Identifier, text, null, startLocation, endLocation);
        }

        public static Token Operator(SyntaxKind kind, string text, Location startLocation, Location endLocation)
        {
            return new Token(kind, text, null, startLocation, endLocation);
        }

        public static Token BoolLiteral(string text, Location startLocation, Location endLocation)
        {
            return new Token(SyntaxKind.BoolLiteral, text, text == "true", startLocation, endLocation);
        }

        public static Token StringLiteral(string text, Location startLocation, Location endLocation)
        {
            return new Token(SyntaxKind.StringLiteral, text.Substring(1, text.Length - 2), text, startLocation, endLocation);
        }

        public static Token CharLiteral(string text, Location startLocation, Location endLocation)
        {
            return new Token(SyntaxKind.CharLiteral, text.Substring(1, text.Length - 2), text, startLocation, endLocation);
        }

        public static Token IntLiteral(string text, Location startLocation, Location endLocation, int radix = 10)
        {
            return new Token(SyntaxKind.IntLiteral, text, Convert.ToInt64(radix == 10 ? text : text.Substring(2), radix), startLocation, endLocation);
        }

        public static Token FloatLiteral(string text, Location startLocation, Location endLocation)
        {
            return new Token(SyntaxKind.FloatLiteral, text, Convert.ToDouble(text), startLocation, endLocation);
        }

        public static Token NoneLiteral(Location startLocation, Location endLocation)
        {
            return new Token(SyntaxKind.NoneLiteral, "none", null, startLocation, endLocation);
        }

        public static Token Trivia(TriviaKind kind, string text, Location startLocation, Location endLocation)
        {
            return new TriviaToken(kind, text, startLocation, endLocation);
        }
    }
}
