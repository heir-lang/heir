using Heir.Syntax;
using Heir.AST;

namespace Heir
{
    public sealed class Parser(TokenStream tokenStream)
    {
        public TokenStream Tokens { get; } = tokenStream.WithoutTrivia(); // temporary

        private DiagnosticBag _diagnostics = tokenStream.Diagnostics;

        public SyntaxTree Parse()
        {
            var statements = new List<Statement>();
            while (!Tokens.IsAtEnd)
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }

            return new(statements, _diagnostics);
        }

        private Statement ParseStatement()
        {
            if (Tokens.Match(SyntaxKind.LetKeyword))
                return ParseVariableDeclaration();

            var expression = ParseExpression();
            return new ExpressionStatement(expression);
        }

        private Statement ParseVariableDeclaration()
        {
            var isMutable = Tokens.Match(SyntaxKind.MutKeyword);
            if (!Tokens.Match(SyntaxKind.Identifier))
            {
                _diagnostics.Error(DiagnosticCode.H004C, $"Expected identifier after 'let', got {Tokens.Current}", Tokens.Previous!);
                return new NoOpStatement();
            }

            var identifier = Tokens.Previous!;
            TypeRef? type = null;
            if (Tokens.Match(SyntaxKind.Colon))
                type = ParseType();

            Expression? initializer = null;
            if (Tokens.Match(SyntaxKind.Equals))
                initializer = ParseExpression();

            if (initializer == null && type == null)
            {
                _diagnostics.Error(DiagnosticCode.H012, $"Cannot infer type of variable '{identifier.Text}', please add an explicit type", identifier);
                return new NoOpStatement();
            }

            return new VariableDeclaration(new IdentifierName(identifier), initializer, type, isMutable);
        }

        private TypeRef ParseType() => ParseSingularType();

        private TypeRef ParseSingularType()
        {
            var token = Tokens.ConsumeType();
            if (token == null)
                return new NoOpType();

            return new SingularType(token);
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
                Tokens.Match(SyntaxKind.SlashSlashEquals) ||
                Tokens.Match(SyntaxKind.PercentEquals) ||
                Tokens.Match(SyntaxKind.CaratEquals) ||
                Tokens.Match(SyntaxKind.AmpersandEquals) ||
                Tokens.Match(SyntaxKind.PipeEquals) ||
                Tokens.Match(SyntaxKind.TildeEquals) ||
                Tokens.Match(SyntaxKind.AmpersandAmpersandEquals) ||
                Tokens.Match(SyntaxKind.PipePipeEquals))
            {
                if (!left.Is<Name>())
                    _diagnostics.Error(DiagnosticCode.H006B, $"Invalid assignment target, expected identifier or member access", left.GetFirstToken());

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
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseLogicalAnd()
        {
            var left = ParseComparison();
            while (Tokens.Match(SyntaxKind.AmpersandAmpersand))
            {
                var op = Tokens.Previous!;
                var right = ParseComparison();
                left = new BinaryOp(left, op, right);
            }

            return left;
        }

        private Expression ParseComparison()
        {
            var left = ParseBitwiseXor();
            while (Tokens.Match(SyntaxKind.EqualsEquals) ||
                   Tokens.Match(SyntaxKind.BangEquals) ||
                   Tokens.Match(SyntaxKind.LT) ||
                   Tokens.Match(SyntaxKind.LTE) ||
                   Tokens.Match(SyntaxKind.GT) ||
                   Tokens.Match(SyntaxKind.GTE))
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
            while (Tokens.Match(SyntaxKind.Plus) || Tokens.Match(SyntaxKind.Minus))
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
            while (Tokens.Match(SyntaxKind.Star) || Tokens.Match(SyntaxKind.Slash) || Tokens.Match(SyntaxKind.SlashSlash) || Tokens.Match(SyntaxKind.Percent))
            {
                var op = Tokens.Previous!;
                var right = ParseExponentiation();
                left = new BinaryOp(left, op, right);
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
                left = new BinaryOp(left, op, right);
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
                    _diagnostics.Error(DiagnosticCode.H006, $"Attempt to {(op.IsKind(SyntaxKind.PlusPlus) ? "in" : "de")}crement a constant, expected identifier", operand.GetFirstToken());

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
                            _diagnostics.Error(DiagnosticCode.H004D, $"Expected expression, got '{Tokens.Previous?.Kind.ToString() ?? "EOF"}'", token);
                            return new NoOp();
                        }

                        Tokens.Consume(SyntaxKind.RParen);
                        return new Parenthesized(expression);
                    }
            }

            _diagnostics.Error(DiagnosticCode.H001B, $"Unexpected token '{token.Kind}'", token);
            return new NoOp();
        }
    }
}
