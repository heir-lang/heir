using System.Collections;
using System.Text;

namespace Heir.Syntax
{
    public class TokenStream(DiagnosticBag diagnostics, Token[] tokens) : IEnumerable<Token>
    {
        public readonly DiagnosticBag Diagnostics = diagnostics;
        public bool IsAtEnd => _index >= _tokens.Length;
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
            var isMatch = Check(kind);
            if (isMatch)
                Advance();

            return isMatch;
        }

        public bool CheckSequential(HashSet<SyntaxKind> kinds, int startOffset = 0)
        {
            var offset = startOffset;
            return kinds.All(kind => Check(kind, offset++));
        }

        public bool CheckSet(HashSet<SyntaxKind> kinds, int offset = 0) => kinds.Any(kind => Check(kind, offset));

        public bool Check(SyntaxKind kind, int offset = 0)
        {
            var token = Peek(offset);
            if (token == null)
                return false;

            return token.IsKind(kind);
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

        public Token? Peek(int offset) => _index + offset >= _tokens.Length ? null : _tokens[_index + offset];

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