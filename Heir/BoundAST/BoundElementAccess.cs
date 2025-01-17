using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundElementAccess(BoundExpression expression, BoundExpression indexExpression) : BoundExpression
{
    public override BaseType Type => Expression.Type is InterfaceType interfaceType
        ? IndexExpression.Type is LiteralType literalType && interfaceType.Members.TryGetValue(literalType, out var member)
            ? member.ValueType
            : IndexExpression.Type is PrimitiveType primitiveType && interfaceType.IndexSignatures.TryGetValue(primitiveType, out var indexSignature)
                ? indexSignature
                : Expression.Type
        : Expression.Type;
    
    public BoundExpression Expression { get; } = expression;
    public BoundExpression IndexExpression { get; } = indexExpression;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundElementAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..IndexExpression.GetTokens()];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundElementAccess(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Expression ->");
        Expression.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Arguments ->");
        IndexExpression.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}