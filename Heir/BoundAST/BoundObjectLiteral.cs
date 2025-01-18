using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundObjectLiteral(Token token, Dictionary<BaseType, BoundExpression> properties, InterfaceType type) : BoundExpression
{
    public Token Token { get; } = token;
    public override InterfaceType Type => type;
    public Dictionary<BaseType, BoundExpression> Properties { get; } = properties;

    public override List<Token> GetTokens() => [Token, ..Properties.Values.SelectMany(expr => expr.GetTokens())];
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundObjectLiteralExpression(this);

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundObjectLiteral(");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Type -> ");
        Console.WriteLine(Type.ToString(false, indent + 1) + ",");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Keys -> [");
        Console.WriteLine(string.Join(",\n", Properties.Keys.Select(key => $"{string.Concat(Enumerable.Repeat("  ", indent + 2))}{key.ToString()}")) );
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}],");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Values -> [");
        var i = 0;
        foreach (var value in Properties.Values)
        {
            value.Display(indent + 2);
            Console.WriteLine(i++ < Properties.Count - 1 ? "," : "");
        }
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}]");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}