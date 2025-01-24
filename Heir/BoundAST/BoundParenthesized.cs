using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundParenthesized(BoundExpression expression) : BoundExpression
{
    public override BaseType Type => Expression.Type;

    public BoundExpression Expression { get; } = expression;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundParenthesizedExpression(this);
    public override List<Token> GetTokens() => Expression.GetTokens();
}