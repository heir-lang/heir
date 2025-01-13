using Heir.Binding;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundParameter(VariableSymbol symbol, BoundExpression? initializer) : BoundExpression
{
    public override BaseType Type => Symbol.Type;

    public VariableSymbol Symbol { get; } = symbol;
    public BoundExpression? Initializer { get; } = initializer;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundParameter(this);
    public override List<Token> GetTokens() => [Symbol.Name, ..Initializer?.GetTokens() ?? []];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundParameter(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Symbol -> {Symbol.ToString()},");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Initializer -> {(Initializer == null ? "none" : "\n")}");
        Initializer?.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}