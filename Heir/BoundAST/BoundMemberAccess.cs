using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundMemberAccess : BoundExpression
{
    public sealed override BaseType Type { get; }

    public BoundExpression Expression { get; }
    public BoundIdentifierName Name { get; }
    
    public BoundMemberAccess(BoundExpression expression, BoundIdentifierName name)
    {
        Expression = expression;
        Name = name;

        if (Expression.Type is InterfaceType interfaceType)
        {
            var literalType = new LiteralType(name.Symbol.Name.Text);
            if (interfaceType.Members.TryGetValue(literalType, out var member))
            {
                Type = member.ValueType;
                return;        
            }
            
            if (interfaceType.IndexSignatures.TryGetValue(literalType.AsPrimitive(), out var primitiveMember))
            {
                Type = primitiveMember;
                return;        
            }
        }
        
        Type = Expression.Type;
    }

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundMemberAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..Name.GetTokens()];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundMemberAccess(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Expression ->");
        Expression.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Name ->");
        Name.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}