using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding;

public enum BoundPostfixOperatorType
{
    NullForgiving
}

public sealed class BoundPostfixOperator
{
    public SyntaxKind SyntaxKind { get; }
    public BoundPostfixOperatorType Type { get; }
    public BaseType OperandType { get; }
    public BaseType ResultType { get; }
    
    private BoundPostfixOperator(SyntaxKind syntaxKind, BoundPostfixOperatorType type)
        : this(syntaxKind, type, IntrinsicTypes.Any, IntrinsicTypes.Any)
    {
    }

    private BoundPostfixOperator(SyntaxKind syntaxKind, BoundPostfixOperatorType type, BaseType operandType)
        : this(syntaxKind, type, operandType, operandType)
    {
    }

    private BoundPostfixOperator(SyntaxKind syntaxKind, BoundPostfixOperatorType type, BaseType operandType, BaseType resultType)
    {
        SyntaxKind = syntaxKind;
        Type = type;
        OperandType = operandType;
        ResultType = resultType;
    }

    private static readonly BoundPostfixOperator[] _operators =
    [
    ];

    public static BoundPostfixOperator? Bind(Token token, BaseType operandType)
    {
        if (token.IsKind(SyntaxKind.Bang))
            return new BoundPostfixOperator(
                token.Kind,
                BoundPostfixOperatorType.NullForgiving,
                IntrinsicTypes.Any,
                BaseType.NonNullable(operandType));
        
        // if (operandType is LiteralType literalType)
        //     operandType = literalType.AsPrimitive();

        return _operators
            .Where(op => token.IsKind(op.SyntaxKind))
            .FirstOrDefault(op => op.OperandType.IsAssignableTo(operandType));
    }
}