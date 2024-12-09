using System.Collections;
using System.Text;

namespace Heir.Syntax
{
    public class TokenStream(DiagnosticBag diagnostics, Token[] tokens) : IEnumerable<Token>
    {
        public readonly DiagnosticBag Diagnostics = diagnostics;
        public Token Current
        {
            get => Peek(0)!;
        }
        public Token? Previous
        {
            get => Peek(-1);
        }

        private readonly Token[] _tokens = (Token[])tokens.Clone();
        private int _index = 0;

        public TokenStream WithoutTrivia()
        {
            return new TokenStream(Diagnostics, _tokens.Where(token => !token.IsKind(SyntaxKind.Trivia)).ToArray());
        }
        
        public bool Match(SyntaxKind kind)
        {
            var token = Current;
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
                Diagnostics.Error("H004", $"Expected {kind} but got {token.Kind}", token);

            return token;
        }

        public Token Advance()
        {
            var token = Current;
            if (token == null)
                Diagnostics.Error("H001B", "End of token stream reached", Previous!);

            _index++;
            return token!;
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