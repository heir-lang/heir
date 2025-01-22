using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class MemberAccess(Expression expression, IdentifierName name) : AssignmentTarget
{
    public Expression Expression { get; } = expression;
    public IdentifierName Name { get; } = name;
    
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitMemberAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..Name.GetTokens()];
}