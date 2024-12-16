using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public sealed class BoundBinaryOperator
    {
        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorType Type { get; }
        public BaseType LeftType { get; }
        public BaseType RightType { get; }
        public BaseType ResultType { get; }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorType type, BaseType nodeType)
            : this(syntaxKind, type, nodeType, nodeType, nodeType)
        {
        }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorType type, BaseType operandType, BaseType resultType)
            : this(syntaxKind, type, operandType, operandType, resultType)
        {
        }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorType type, BaseType leftType, BaseType rightType, BaseType resultType)
        {
            SyntaxKind = syntaxKind;
            Type = type;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.Plus, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Plus, BoundBinaryOperatorType.Addition, IntrinsicTypes.StringOrChar),
            new BoundBinaryOperator(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Addition, IntrinsicTypes.StringOrChar),
            new BoundBinaryOperator(SyntaxKind.PlusPlus, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Minus, BoundBinaryOperatorType.Subtraction, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.MinusEquals, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.MinusMinus, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Star, BoundBinaryOperatorType.Multiplication, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.StarEquals, BoundBinaryOperatorType.Multiplication, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Slash, BoundBinaryOperatorType.Division, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.SlashEquals, BoundBinaryOperatorType.Division, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.SlashSlash, BoundBinaryOperatorType.IntegerDivision, IntrinsicTypes.Number, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.SlashSlashEquals, BoundBinaryOperatorType.IntegerDivision, IntrinsicTypes.Number, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.Carat, BoundBinaryOperatorType.Exponentation, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.CaratEquals, BoundBinaryOperatorType.Exponentation, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Percent, BoundBinaryOperatorType.Modulus, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.PercentEquals, BoundBinaryOperatorType.Modulus, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Ampersand, BoundBinaryOperatorType.BitwiseAnd, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.AmpersandEquals, BoundBinaryOperatorType.BitwiseAnd, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Pipe, BoundBinaryOperatorType.BitwiseOr, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.PipeEquals, BoundBinaryOperatorType.BitwiseOr, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.Tilde, BoundBinaryOperatorType.BitwiseXor, IntrinsicTypes.Number),
            new BoundBinaryOperator(SyntaxKind.TildeEquals, BoundBinaryOperatorType.BitwiseXor, IntrinsicTypes.Number),

            new BoundBinaryOperator(SyntaxKind.EqualsEquals, BoundBinaryOperatorType.Equals, IntrinsicTypes.Number, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEquals, BoundBinaryOperatorType.NotEquals, IntrinsicTypes.Number, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.EqualsEquals, BoundBinaryOperatorType.Equals, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEquals, BoundBinaryOperatorType.NotEquals, PrimitiveType.Bool),

            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersand, BoundBinaryOperatorType.LogicalAnd, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.PipePipe, BoundBinaryOperatorType.LogicalOr, PrimitiveType.Bool)
        };

        public static BoundBinaryOperator? Bind(Token token, BaseType leftType, BaseType rightType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind != token.Kind) continue;
                if (!op.LeftType.IsAssignableTo(leftType)) continue;
                if (!op.RightType.IsAssignableTo(rightType)) continue;

                return op;
            }

            return null;
        }
    }
}
