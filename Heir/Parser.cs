using Heir.Syntax;
using Heir.AST;

namespace Heir
{
    public sealed class Parser(TokenStream tokenStream)
    {
        public TokenStream Tokens { get; } = tokenStream.WithoutTrivia(); // temporary
        public DiagnosticBag Diagnostics { get; } = tokenStream.Diagnostics;

        public SyntaxTree Parse()
        {
            var statement = ParseExpression(); // TODO: yeah you know what to do lol
            return new([statement]);
        }

        private Expression ParseExpression() => ParseAssignment();

        private Expression ParseAssignment()
        {
            var left = ParseLogicalOr();
            if (Tokens.Match(SyntaxKind.Equals) ||
                Tokens.Match(SyntaxKind.PlusEquals) ||
                Tokens.Match(SyntaxKind.MinusEquals) ||
                Tokens.Match(SyntaxKind.StarEquals) ||
                Tokens.Match(SyntaxKind.SlashEquals) ||
                Tokens.Match(SyntaxKind.PercentEquals) ||
                Tokens.Match(SyntaxKind.CaratEquals) ||
                Tokens.Match(SyntaxKind.AmpersandEquals) ||
                Tokens.Match(SyntaxKind.PipeEquals) ||
                Tokens.Match(SyntaxKind.TildeEquals) ||
                Tokens.Match(SyntaxKind.AmpersandAmpersandEquals) ||
                Tokens.Match(SyntaxKind.PipePipeEquals))
            {
                if (!left.Is<IdentifierName>()) // && !left.Is<MemberAccess>()
                    Diagnostics.Error("H008", $"Invalid assignment target, expected identifier or member access", left.GetFirstToken());

                var op = Tokens.Previous!;
                var right = ParseAssignment();
                return new AssignmentOp(left, op, right);
            }

            return left;
        }

        private Expression ParseLogicalOr()
        {
            var left = ParseLogicalAnd();
            while (Tokens.Match(SyntaxKind.PipePipe))
            {
                var op = Tokens.Previous!;
                var right = ParseLogicalAnd();
                left = new BoundBinaryOp(left, op, right);
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
                left = new BoundBinaryOp(left, op, right);
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
                left = new BoundBinaryOp(left, op, right);
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
                left = new BoundBinaryOp(left, op, right);
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
                left = new BoundBinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseAddition()
        {
            var left = ParseMultiplication();
            while (Tokens.Match(SyntaxKind.Plus) || Tokens.Match(SyntaxKind.Minus))
            {
                var op = Tokens.Previous!;
                var right = ParseMultiplication();
                left = new BoundBinaryOp(left, op, right);
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
                left = new BoundBinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseExponentiation()
        {
            var left = ParseUnary();
            while (Tokens.Match(SyntaxKind.Carat))
            {
                var op = Tokens.Previous!;
                var right = ParseUnary();
                left = new BoundBinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseUnary()
        {
            if (Tokens.Match(SyntaxKind.Bang) ||
                Tokens.Match(SyntaxKind.Tilde) ||
                Tokens.Match(SyntaxKind.PlusPlus) ||
                Tokens.Match(SyntaxKind.MinusMinus) ||
                Tokens.Match(SyntaxKind.Minus))
            {
                var op = Tokens.Previous!;
                var operand = ParseUnary(); // recursively parse the operand
                var isAssignmentOp = op.IsKind(SyntaxKind.PlusPlus) || op.IsKind(SyntaxKind.MinusMinus);
                if (isAssignmentOp && operand.Is<Literal>())
                    Diagnostics.Error("H006", $"Attempt to {(op.IsKind(SyntaxKind.PlusPlus) ? "in" : "de")}crement a constant, expected identifier", operand.GetFirstToken());

                return new UnaryOp(operand, op);
            }

            return ParsePrimary();
        }

        private Expression ParsePrimary()
        {
            var token = Tokens.Advance();
            if (token == null)
                return new NoOp();

            switch (token.Kind)
            {
                case SyntaxKind.BoolLiteral:
                case SyntaxKind.CharLiteral:
                case SyntaxKind.StringLiteral:
                case SyntaxKind.IntLiteral:
                case SyntaxKind.FloatLiteral:
                case SyntaxKind.NoneKeyword:
                    return new Literal(token);

                case SyntaxKind.Identifier:
                    return new IdentifierName(token);

                case SyntaxKind.LParen:
                    {
                        var expression = ParseExpression();
                        if (expression.Is<NoOp>())
                        {
                            Diagnostics.Error("H007", $"Expected expression, got {(Tokens.Previous?.Kind.ToString() ?? "EOF")}", token);
                            return new NoOp();
                        }

                        Tokens.Consume(SyntaxKind.RParen);
                        return new Parenthesized(expression);
                    }
            }

            Diagnostics.Error("H005", $"Unexpected token \"{token.Kind}\"", token);
            return new NoOp();
        }
    }
}
