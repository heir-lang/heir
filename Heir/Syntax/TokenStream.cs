using System.Collections;
using System.Text;

namespace Heir.Syntax
{
    public class TokenStream(DiagnosticBag diagnostics, Token[] tokens) : IEnumerable<Token>
    {
        public readonly DiagnosticBag Diagnostics = diagnostics;
        public bool IsAtEnd => _tokens.ElementAtOrDefault(_index) == null;
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

        public Token? ConsumeType()
        {
            var token = Advance();
            if (token != null)
                foreach (var typeKind in SyntaxFacts.TypeSyntaxes)
                {
                    if (!token.IsKind(typeKind)) continue;
                    return token;
                }

            Diagnostics.Error(DiagnosticCode.H004B, $"Expected type, got '{token?.Kind.ToString() ?? "EOF"}'", token ?? Peek(-2)!);
            return null;
        }

        public Token? Consume(SyntaxKind kind)
        {
            var token = Advance();
            if (token == null || !token.IsKind(kind))
                Diagnostics.Error(DiagnosticCode.H004, $"Expected {kind}, got '{token?.Kind.ToString() ?? "EOF"}'", token ?? Peek(-2)!);

            return token;
        }

        public Token? Advance()
        {
            var token = Current;
            _index++;
            return token;
        }

        public Token? Peek(int offset) => _tokens.ElementAtOrDefault(_index + offset);

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var token in this)
                result.AppendLine(token.ToString());

            return result.ToString().TrimEnd();
        }

        public IEnumerator<Token> GetEnumerator() => ((IEnumerable<Token>)_tokens).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _tokens.GetEnumerator();
    }
}