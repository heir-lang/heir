using Heir.Binding;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundVariableDeclaration(VariableSymbol<BaseType> symbol, BoundExpression? initializer, bool isMutable) : BoundStatement
{
    public override BaseType? Type => Symbol.Type;

    public VariableSymbol<BaseType> Symbol { get; } = symbol;
    public BoundExpression? Initializer { get; } = initializer;
    public bool IsMutable { get; } = isMutable;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundVariableDeclaration(this);
    public override List<Token> GetTokens() => [Symbol.Name, ..Initializer?.GetTokens() ?? []];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundVariableDeclaration(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Symbol -> {Symbol.ToString()},");
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Initializer -> {(Initializer == null ? "none" : "\n")}");
        Initializer?.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Mutable: {IsMutable}");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}