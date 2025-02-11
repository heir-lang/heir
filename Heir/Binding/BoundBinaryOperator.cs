using Heir.CodeGeneration;
using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding;

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
    BitShiftLeft,
    BitShiftRight,
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
        { BoundBinaryOperatorType.BitShiftLeft,        OpCode.BSHL },
        { BoundBinaryOperatorType.BitShiftRight,       OpCode.BSHR },
        { BoundBinaryOperatorType.LogicalAnd,          OpCode.AND },
        { BoundBinaryOperatorType.LogicalOr,           OpCode.OR },
        { BoundBinaryOperatorType.Equals,              OpCode.EQ },
        { BoundBinaryOperatorType.NotEquals,           OpCode.NEQ },
        { BoundBinaryOperatorType.LessThan,            OpCode.LT },
        { BoundBinaryOperatorType.LessThanOrEquals,    OpCode.LTE },
        { BoundBinaryOperatorType.GreaterThan,         OpCode.GT },
        { BoundBinaryOperatorType.GreaterThanOrEquals, OpCode.GTE },
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

    private static readonly BoundBinaryOperator[] _operators =
    [
        new(SyntaxKind.Plus, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
        new(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
        new(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.String),
        new(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.String),
        new(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char),
        new(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char),
        new(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.String, PrimitiveType.Char, IntrinsicTypes.StringOrChar),
        new(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.String, PrimitiveType.Char, IntrinsicTypes.StringOrChar),
        new(SyntaxKind.Plus, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char, PrimitiveType.String, IntrinsicTypes.StringOrChar),
        new(SyntaxKind.PlusEquals, BoundBinaryOperatorType.Concatenation, PrimitiveType.Char, PrimitiveType.String, IntrinsicTypes.StringOrChar),
        new(SyntaxKind.PlusPlus, BoundBinaryOperatorType.Addition, IntrinsicTypes.Number),
        new(SyntaxKind.Minus, BoundBinaryOperatorType.Subtraction, IntrinsicTypes.Number),
        new(SyntaxKind.MinusEquals, BoundBinaryOperatorType.Subtraction, IntrinsicTypes.Number),
        new(SyntaxKind.MinusMinus, BoundBinaryOperatorType.Subtraction, IntrinsicTypes.Number),
        new(SyntaxKind.Star, BoundBinaryOperatorType.Multiplication, IntrinsicTypes.Number),
        new(SyntaxKind.StarEquals, BoundBinaryOperatorType.Multiplication, IntrinsicTypes.Number),
        new(SyntaxKind.Slash, BoundBinaryOperatorType.Division, IntrinsicTypes.Number),
        new(SyntaxKind.SlashEquals, BoundBinaryOperatorType.Division, IntrinsicTypes.Number),
        new(SyntaxKind.SlashSlash, BoundBinaryOperatorType.IntegerDivision, IntrinsicTypes.Number, PrimitiveType.Int),
        new(SyntaxKind.SlashSlashEquals, BoundBinaryOperatorType.IntegerDivision, IntrinsicTypes.Number, PrimitiveType.Int),
        new(SyntaxKind.Carat, BoundBinaryOperatorType.Exponentation, IntrinsicTypes.Number),
        new(SyntaxKind.CaratEquals, BoundBinaryOperatorType.Exponentation, IntrinsicTypes.Number),
        new(SyntaxKind.Percent, BoundBinaryOperatorType.Modulus, IntrinsicTypes.Number),
        new(SyntaxKind.PercentEquals, BoundBinaryOperatorType.Modulus, IntrinsicTypes.Number),
        new(SyntaxKind.Ampersand, BoundBinaryOperatorType.BitwiseAnd, PrimitiveType.Int),
        new(SyntaxKind.AmpersandEquals, BoundBinaryOperatorType.BitwiseAnd, PrimitiveType.Int),
        new(SyntaxKind.Pipe, BoundBinaryOperatorType.BitwiseOr, PrimitiveType.Int),
        new(SyntaxKind.PipeEquals, BoundBinaryOperatorType.BitwiseOr, PrimitiveType.Int),
        new(SyntaxKind.Tilde, BoundBinaryOperatorType.BitwiseXor, PrimitiveType.Int),
        new(SyntaxKind.TildeEquals, BoundBinaryOperatorType.BitwiseXor, PrimitiveType.Int),
        new(SyntaxKind.LArrowLArrow, BoundBinaryOperatorType.BitShiftLeft, PrimitiveType.Int),
        new(SyntaxKind.LArrowLArrowEquals, BoundBinaryOperatorType.BitShiftLeft, PrimitiveType.Int),
        new(SyntaxKind.RArrowRArrow, BoundBinaryOperatorType.BitShiftRight, PrimitiveType.Int),
        new(SyntaxKind.RArrowRArrowEquals, BoundBinaryOperatorType.BitShiftRight, PrimitiveType.Int),
            
        new(SyntaxKind.EqualsEquals, BoundBinaryOperatorType.Equals, IntrinsicTypes.Any, PrimitiveType.Bool),
        new(SyntaxKind.BangEquals, BoundBinaryOperatorType.NotEquals, IntrinsicTypes.Any, PrimitiveType.Bool),
        new(SyntaxKind.LT, BoundBinaryOperatorType.LessThan, IntrinsicTypes.Number, PrimitiveType.Bool),
        new(SyntaxKind.LTE, BoundBinaryOperatorType.LessThanOrEquals, IntrinsicTypes.Number, PrimitiveType.Bool),
        new(SyntaxKind.GT, BoundBinaryOperatorType.GreaterThan, IntrinsicTypes.Number, PrimitiveType.Bool),
        new(SyntaxKind.GTE, BoundBinaryOperatorType.GreaterThanOrEquals, IntrinsicTypes.Number, PrimitiveType.Bool),

        new(SyntaxKind.AmpersandAmpersand, BoundBinaryOperatorType.LogicalAnd, PrimitiveType.Bool),
        new(SyntaxKind.PipePipe, BoundBinaryOperatorType.LogicalOr, PrimitiveType.Bool),

        new(SyntaxKind.Equals, BoundBinaryOperatorType.Assignment, IntrinsicTypes.Any)
    ];

    public static BoundBinaryOperator? Bind(Token token, BaseType leftType, BaseType rightType)
    {
        if (leftType is LiteralType leftLiteral)
            leftType = leftLiteral.AsPrimitive();
            
        if (rightType is LiteralType rightLiteral)
            rightType = rightLiteral.AsPrimitive();

        return _operators
            .Where(op => token.IsKind(op.SyntaxKind))
            .Where(op => op.LeftType.IsAssignableTo(leftType))
            .FirstOrDefault(op => op.RightType.IsAssignableTo(rightType));
    }
}