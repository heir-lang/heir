using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundInvocation(BoundExpression callee, List<BoundExpression> arguments, List<BaseType> typeArguments) : BoundExpression
{
    public override BaseType Type => Callee.Type is FunctionType functionType
        ? functionType.ReturnType
        : Callee.Type;

    public BoundExpression Callee { get; } = callee;
    public List<BoundExpression> Arguments { get; } = arguments;
    public List<BaseType> TypeArguments { get; } = typeArguments;
    public bool IsIntrinsic { get; } = callee is BoundIdentifierName { Symbol.IsIntrinsic: true };

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundInvocationExpression(this);
    public override List<Token> GetTokens() => [..Callee.GetTokens(), ..Arguments.SelectMany(argument => argument.GetTokens())];
    
    public BoundInvocation WithTypeArguments(List<BaseType> typeArguments) => new(Callee, Arguments, typeArguments);
}