using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundElementAccess : BoundExpression
{
    public sealed override BaseType Type { get; }

    public BoundExpression Expression { get; }
    public BoundExpression IndexExpression { get; }
    
    public BoundElementAccess(BoundExpression expression, BoundExpression indexExpression)
    {
        Expression = expression;
        IndexExpression = indexExpression;

        if (Expression.Type is InterfaceType interfaceType)
        {
            Type = GetTypeAtIndex(interfaceType);
            return;
        }
        
        Type = Expression.Type;
    }

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundElementAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..IndexExpression.GetTokens()];
    
    private BaseType GetTypeAtIndex(InterfaceType interfaceType) =>
        IndexExpression.Type switch
        {
            LiteralType literalType when interfaceType.Members.TryGetValue(literalType, out var member) => member.Type,
                
            LiteralType literalType when
                interfaceType.IndexSignatures.TryGetValue(literalType.AsPrimitive(), out var primitiveMember) => primitiveMember,
                
            PrimitiveType primitiveType when 
                interfaceType.IndexSignatures.TryGetValue(primitiveType, out var indexSignature) => indexSignature,
                
            _ => Expression.Type
        };
}