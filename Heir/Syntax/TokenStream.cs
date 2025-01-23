﻿using System.Collections;
using System.Text;

namespace Heir.Syntax
{
    public class TokenStream(DiagnosticBag diagnostics, List<Token> tokens) : IEnumerable<Token>
    {
        public readonly DiagnosticBag Diagnostics = diagnostics;
        public bool IsAtEnd => Position >= tokens.Count;
        public Token Current => Peek(0)!;
        public Token? Previous => Peek(-1);
        public int Position { get; private set; }

        public TokenStream WithoutTrivia()
        {
            return new TokenStream(Diagnostics, tokens.FindAll(token => !token.IsKind(SyntaxKind.Trivia)));
        }
        
        public bool Match(SyntaxKind kind) => Match(kind, out _);

        public bool Match(SyntaxKind kind, out Token matchedToken)
        {
            var isMatch = Check(kind);
            if (isMatch)
                Advance();

            matchedToken = Previous!;
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
            return token != null && token.IsKind(kind);
        }

        public Token? ConsumeType()
        {
            var token = Advance();
            if (SyntaxFacts.TypeSyntaxes.Any(typeKind => token.IsKind(typeKind)))
                return token;

            Diagnostics.Error(DiagnosticCode.H004B,
                $"Expected type, got '{token?.Kind.ToString() ?? "EOF"}'",
                token ?? Peek(-2)!);
            
            return null;
        }

        public Token? Consume(SyntaxKind kind)
        {
            var token = Advance();
            if (token == null || !token.IsKind(kind))
            {
                var invalidToken = token ?? Peek(-2) ?? Peek(-3)!;
                var got = SyntaxFacts.OperatorMap.Contains(invalidToken.Kind)
                    ? SyntaxFacts.OperatorMap.GetKey(invalidToken.Kind)
                    : SyntaxFacts.KeywordMap.Contains(invalidToken.Kind)
                        ? SyntaxFacts.KeywordMap.GetKey(invalidToken.Kind)
                        : invalidToken.Kind.ToString();
                
                Diagnostics.Error(DiagnosticCode.H004,
                    $"Expected {kind}, got '{got}'",
                    invalidToken);
            }

            return token;
        }

        public Token Advance()
        {
            var token = Current;
            Position++;
            return token;
        }

        public Token? Peek(int offset) => tokens.ElementAtOrDefault(Position + offset);

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var token in this)
                result.AppendLine(token.ToString());

            return result.ToString().TrimEnd();
        }

        public IEnumerator<Token> GetEnumerator() => ((IEnumerable<Token>)tokens).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => tokens.GetEnumerator();
    }
}