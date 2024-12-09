using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Heir.Syntax
{
    internal class EndOfTokenStreamException : Exception
    {
        public EndOfTokenStreamException()
            : base("End of token stream reached")
        {
        }
    }

    public class TokenStream(DiagnosticBag diagnostics, Token[] tokens) : IEnumerable<Token>
    {
        private readonly DiagnosticBag _diagnostics = diagnostics;
        private readonly Token[] _tokens = (Token[])tokens.Clone();
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
            {
                _diagnostics.Error("H003", $"Expected {kind} but got {token.Kind}", token);
            }

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
            return _tokens.ElementAtOrDefault(_index + offset);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var token in this)
                result.AppendLine(token.ToString());

            return result.ToString().TrimEnd();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return ((IEnumerable<Token>)_tokens).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }
    }
}