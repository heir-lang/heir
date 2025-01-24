using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ExpressionStatement(Expression expression) : Statement
{
    public Expression Expression { get; } = expression;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitExpressionStatement(this);
    public override List<Token> GetTokens() => Expression.GetTokens();
}