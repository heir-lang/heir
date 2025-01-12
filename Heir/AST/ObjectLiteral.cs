using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ObjectLiteral(Token token, Dictionary<Expression, Expression> properties) : Literal(token)
{
    public Dictionary<Expression, Expression> Properties { get; } = properties;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitObjectLiteralExpression(this);

    public override void Display(int indent)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}ObjectLiteral(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Keys -> [");
        foreach (var key in Properties.Keys)
        {
            key.Display(indent + 2);
            Console.WriteLine(",");
        }
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}],");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Values -> [");
        foreach (var value in Properties.Values)
        {
            value.Display(indent + 2);
            Console.WriteLine(",");
        }
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}]");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}