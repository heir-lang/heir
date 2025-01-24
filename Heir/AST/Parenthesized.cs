using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Parenthesized(Expression expression) : Expression
{
    public Expression Expression { get; } = expression;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitParenthesizedExpression(this);
    public override List<Token> GetTokens() => Expression.GetTokens();
}