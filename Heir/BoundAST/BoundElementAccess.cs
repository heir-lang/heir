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
            Type = IndexExpression.Type switch
            {
                LiteralType literalType when interfaceType.Members.TryGetValue(literalType, out var member) => member.ValueType,
                
                LiteralType literalType when
                    interfaceType.IndexSignatures.TryGetValue(literalType.AsPrimitive(), out var primitiveMember) => primitiveMember,
                
                PrimitiveType primitiveType when 
                    interfaceType.IndexSignatures.TryGetValue(primitiveType, out var indexSignature) => indexSignature,
                
                _ => Expression.Type
            };

            return;
        }
        
        Type = Expression.Type;
    }

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundElementAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..IndexExpression.GetTokens()];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundElementAccess(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Expression ->");
        Expression.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}IndexExpression ->");
        IndexExpression.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}