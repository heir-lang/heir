using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundMemberAccess : BoundAssignmentTarget
{
    public override BaseType Type { get; }

    public BoundExpression Expression { get; }
    public BoundIdentifierName Name { get; }
    
    public BoundMemberAccess(BoundExpression expression, BoundIdentifierName name)
    {
        Expression = expression;
        Name = name;

        if (Expression.Type is InterfaceType interfaceType && GetMemberType(interfaceType) is { } memberType)
        {
            Type = memberType;
            return;
        }
        
        Type = Expression.Type;
    }

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundMemberAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..Name.GetTokens()];

    private BaseType? GetMemberType(InterfaceType interfaceType)
    {
        var literalType = new LiteralType(Name.Symbol.Name.Text);
        if (interfaceType.Members.TryGetValue(literalType, out var member))
            return member.Type;
            
        if (interfaceType.IndexSignatures.TryGetValue(literalType.AsPrimitive(), out var primitiveMember))
            return primitiveMember;
        
        return null;
    }
}