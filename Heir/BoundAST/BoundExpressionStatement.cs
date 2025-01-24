using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundExpressionStatement(BoundExpression expression) : BoundStatement
{
    public override BaseType Type => Expression.Type;

    public BoundExpression Expression { get; } = expression;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundExpressionStatement(this);
    public override List<Token> GetTokens() => [];
}