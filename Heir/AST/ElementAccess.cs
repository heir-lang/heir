using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ElementAccess(Expression expression, Expression indexExpression) : AssignmentTarget
{
    public Expression Expression { get; } = expression;
    public Expression IndexExpression { get; } = indexExpression;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitElementAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..IndexExpression.GetTokens()];

}