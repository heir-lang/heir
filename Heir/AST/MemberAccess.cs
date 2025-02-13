using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class MemberAccess(Expression expression, IdentifierName name) : AssignmentTarget
{
    public Expression Expression { get; } = expression;
    public IdentifierName Name { get; } = name;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitMemberAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..Name.GetTokens()];
}