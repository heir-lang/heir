using Heir.Syntax;
using Heir.AST;

namespace Heir
{
    public class Parser(TokenStream tokenStream)
    {
        public TokenStream Tokens { get; } = tokenStream.WithoutTrivia(); // temporary
        public DiagnosticBag Diagnostics { get; } = tokenStream.Diagnostics;

        public Expression ParseExpression() => ParseLogicalOr();

        private Expression ParseLogicalOr()
        {
            var left = ParseLogicalAnd();
            while (Tokens.Match(SyntaxKind.PipePipe))
            {
                var op = Tokens.Previous!;
                var right = ParseLogicalAnd();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseLogicalAnd()
        {
            var left = ParseBitwiseXor();
            while (Tokens.Match(SyntaxKind.AmpersandAmpersand))
            {
                var op = Tokens.Previous!;
                var right = ParseBitwiseXor();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseBitwiseXor()
        {
            var left = ParseBitwiseOr();
            while (Tokens.Match(SyntaxKind.Tilde))
            {
                var op = Tokens.Previous!;
                var right = ParseBitwiseOr();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseBitwiseOr()
        {
            var left = ParseBitwiseAnd();
            while (Tokens.Match(SyntaxKind.Pipe))
            {
                var op = Tokens.Previous!;
                var right = ParseBitwiseAnd();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseBitwiseAnd()
        {
            var left = ParseAddition();
            while (Tokens.Match(SyntaxKind.Ampersand))
            {
                var op = Tokens.Previous!;
                var right = ParseAddition();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

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
            var left = ParseExponentiation();
            while (Tokens.Match(SyntaxKind.Star) || Tokens.Match(SyntaxKind.Slash) || Tokens.Match(SyntaxKind.Percent))
            {
                var op = Tokens.Previous!;
                var right = ParseExponentiation();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseExponentiation()
        {
            var left = ParsePrimary();
            while (Tokens.Match(SyntaxKind.Carat))
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
                case SyntaxKind.NoneKeyword:
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