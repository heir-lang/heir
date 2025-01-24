using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundInvocation(BoundExpression callee, List<BoundExpression> arguments) : BoundExpression
{
    public override BaseType Type => Callee.Type is FunctionType functionType
        ? functionType.ReturnType
        : Callee.Type;

    public BoundExpression Callee { get; } = callee;
    public List<BoundExpression> Arguments { get; } = arguments;
    public bool IsIntrinsic { get; } = callee is BoundIdentifierName { Symbol.IsIntrinsic: true };

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundInvocationExpression(this);
    public override List<Token> GetTokens() => [..Callee.GetTokens(), ..Arguments.SelectMany(argument => argument.GetTokens())];
}