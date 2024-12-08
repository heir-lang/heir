namespace Heir.Syntax
{
    public static class TokenFactory
    {
        public static Token Operator(SyntaxKind kind, string text, Location location)
        {
            return new Token(kind, text, null, location);
        }

        public static Token BoolLiteral(bool value, Location location)
        {
            return new Token(SyntaxKind.BoolLiteral, value.ToString().ToLower(), value, location);
        }

        public static Token StringLiteral(string text, Location location)
        {
            return new Token(SyntaxKind.StringLiteral, text, text, location);
        }

        public static Token IntLiteral(string text, Location location)
        {
            return new Token(SyntaxKind.IntLiteral, text, Convert.ToInt64(text), location);
        }

        public static Token FloatLiteral(string text, Location location)
        {
            return new Token(SyntaxKind.FloatLiteral, text, Convert.ToDouble(text), location);
        }
    }
}
