using Heir.Syntax;
using Heir.AST;

namespace Heir
{
    public class Parser(TokenStream tokenStream)
    {
        public TokenStream Tokens { get; } = tokenStream;
        public DiagnosticBag Diagnostics { get; } = tokenStream.Diagnostics;

        public Expression ParseExpression() => ParseAddition();

        private Expression ParseAddition()
        {
            var left = ParseMultiplication();
            while (Tokens.Match(SyntaxKind.Plus) || Tokens.Match(SyntaxKind.Slash))
            {
                var op = Tokens.Previous!;
                var right = ParseMultiplication();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseMultiplication()
        {
            var left = ParsePrimary();
            while (Tokens.Match(SyntaxKind.Star) || Tokens.Match(SyntaxKind.Slash))
            {
                var op = Tokens.Previous!;
                var right = ParsePrimary();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParsePrimary()
        {
            var token = Tokens.Advance();
            switch (token.Kind)
            {
                case SyntaxKind.BoolLiteral:
                case SyntaxKind.CharLiteral:
                case SyntaxKind.StringLiteral:
                case SyntaxKind.IntLiteral:
                case SyntaxKind.FloatLiteral:
                case SyntaxKind.NoneLiteral:
                    return new Literal(token);

                case SyntaxKind.LParen:
                    {
                        var expression = ParseExpression();
                        Tokens.Consume(SyntaxKind.RParen);

                        return new Parenthesized(expression);
                    }
            }

            Diagnostics.Error("H005", $"Unexpected token \"{token.Kind}\"", token);
            return new NoOp();
        }
    }
}