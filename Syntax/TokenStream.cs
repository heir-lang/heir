namespace Heir.Syntax
{
    internal class EndOfTokenStreamException : Exception
    {
        public EndOfTokenStreamException()
            : base("End of token stream reached")
        {
        }
    }

    internal class InvalidConsumptionException : Exception
    {
        public InvalidConsumptionException(SyntaxKind expected, SyntaxKind got)
            : base($"Invalid token consumption: Expected {expected} but got {got}")
        {
        }
    }

    internal class TokenStream(Token[] tokens)
    {
        public Token[] Tokens { get; } = (Token[])tokens.Clone();
        private int _index = 0;

        public bool Match(SyntaxKind kind)
        {
            var token = Current();
            if (token == null) return false;

            var isMatch = token.IsKind(kind);
            if (isMatch)
                Advance();

            return isMatch;
        }

        public Token Consume(SyntaxKind kind)
        {
            var token = Advance();
            if (!token.IsKind(kind))
                throw new InvalidConsumptionException(kind, token.Kind);

            return token;
        }

        public Token Advance()
        {
            var token = Current();
            if (token == null)
                throw new EndOfTokenStreamException();

            _index++;
            return token;
        }

        public Token? Current()
        {
            return Peek(0);
        }

        public Token? Previous()
        {
            return Peek(-1);
        }

        public Token? Peek(int offset)
        {
            return Tokens.ElementAt(_index + offset);
        }
    }
}