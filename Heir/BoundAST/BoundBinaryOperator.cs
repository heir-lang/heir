using Heir.Syntax;
using Heir.Types;
using Heir.CodeGeneration;

namespace Heir.BoundAST
{
    public enum BoundBinaryOperatorType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        IntegerDivision,
        Exponentation,
        Modulus,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        LogicalOr,
        LogicalAnd,
        Equals,
        NotEquals,
        LessThan,
        LessThanOrEquals,
        GreaterThan,
        GreaterThanOrEquals,
        Concatenation,
        Assignment
    }

    public sealed class BoundBinaryOperator
    {
        public static readonly Dictionary<BoundBinaryOperatorType, OpCode> OpCodeMap = new()
        {
            { BoundBinaryOperatorType.Addition,            OpCode.ADD },
            { BoundBinaryOperatorType.Subtraction,         OpCode.SUB },
            { BoundBinaryOperatorType.Multiplication,      OpCode.MUL },
            { BoundBinaryOperatorType.Division,            OpCode.DIV },
            { BoundBinaryOperatorType.IntegerDivision,     OpCode.IDIV },
            { BoundBinaryOperatorType.Modulus,             OpCode.MOD },
            { BoundBinaryOperatorType.Exponentation,       OpCode.POW },
            { BoundBinaryOperatorType.BitwiseAnd,          OpCode.BAND },
            { BoundBinaryOperatorType.BitwiseOr,           OpCode.BOR },
            { BoundBinaryOperatorType.BitwiseXor,          OpCode.BXOR },
            { BoundBinaryOperatorType.LogicalAnd,          OpCode.AND },
            { BoundBinaryOperatorType.LogicalOr,           OpCode.OR },
            { BoundBinaryOperatorType.Equals,              OpCode.EQ },
            { BoundBinaryOperatorType.NotEquals,           OpCode.EQ },
            { BoundBinaryOperatorType.LessThan,            OpCode.LT },
            { BoundBinaryOperatorType.LessThanOrEquals,    OpCode.LTE },
            { BoundBinaryOperatorType.GreaterThan,         OpCode.LT },
            { BoundBinaryOperatorType.GreaterThanOrEquals, OpCode.LTE },
            { BoundBinaryOperatorType.Concatenation,       OpCode.CONCAT }
        };

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
            new BoundBinaryOperator(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.String),
            new BoundBinaryOperator(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.String),
            new BoundBinaryOperator(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char),
            new BoundBinaryOperator(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char),
            new BoundBinaryOperator(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.String, PrimitiveType.Char, IntrinsicTypes.StringOrChar),
            new BoundBinaryOperator(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.String, PrimitiveType.Char, IntrinsicTypes.StringOrChar),
            new BoundBinaryOperator(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char, PrimitiveType.String, IntrinsicTypes.StringOrChar),
            new BoundBinaryOperator(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char, PrimitiveType.String, IntrinsicTypes.StringOrChar),
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
            new BoundBinaryOperator(SyntaxKind.Ampersand, BoundBinaryOperatorType.BitwiseAnd, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.AmpersandEquals, BoundBinaryOperatorType.BitwiseAnd, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.Pipe, BoundBinaryOperatorType.BitwiseOr, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.PipeEquals, BoundBinaryOperatorType.BitwiseOr, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.Tilde, BoundBinaryOperatorType.BitwiseXor, PrimitiveType.Int),
            new BoundBinaryOperator(SyntaxKind.TildeEquals, BoundBinaryOperatorType.BitwiseXor, PrimitiveType.Int),

            new BoundBinaryOperator(SyntaxKind.EqualsEquals, BoundBinaryOperatorType.Equals, IntrinsicTypes.Any, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEquals, BoundBinaryOperatorType.NotEquals, IntrinsicTypes.Any, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.LT, BoundBinaryOperatorType.LessThan, IntrinsicTypes.Number, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.LTE, BoundBinaryOperatorType.LessThanOrEquals, IntrinsicTypes.Number, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.GT, BoundBinaryOperatorType.GreaterThan, IntrinsicTypes.Number, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.GTE, BoundBinaryOperatorType.GreaterThanOrEquals, IntrinsicTypes.Number, PrimitiveType.Bool),

            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersand, BoundBinaryOperatorType.LogicalAnd, PrimitiveType.Bool),
            new BoundBinaryOperator(SyntaxKind.PipePipe, BoundBinaryOperatorType.LogicalOr, PrimitiveType.Bool),

            new BoundBinaryOperator(SyntaxKind.Equals, BoundBinaryOperatorType.Assignment, IntrinsicTypes.Any)
        };

        public static BoundBinaryOperator? Bind(Token token, BaseType leftType, BaseType rightType)
        {
            foreach (var op in _operators)
            {
                if (!token.IsKind(op.SyntaxKind)) continue;
                if (!op.LeftType.IsAssignableTo(leftType)) continue;
                if (!op.RightType.IsAssignableTo(rightType)) continue;

                return op;
            }

            return null;
        }
    }
}
