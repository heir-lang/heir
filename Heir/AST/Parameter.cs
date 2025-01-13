using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class Parameter(IdentifierName name, TypeRef? type, Expression? initializer) : Expression
{
    public IdentifierName Name { get; } = name;
    public TypeRef? Type { get; } = type;
    public Expression? Initializer { get; } = initializer;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitParameter(this);
    public override List<Token> GetTokens() => Name.GetTokens().Concat(Initializer?.GetTokens() ?? []).ToList();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Parameter(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Name ->");
        Name.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Type -> {(Type == null ? "(inferred)" : "\n")}");
        Type?.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Initializer -> {(Initializer == null ? "none" : "\n")}");
        Initializer?.Display(indent + 2);
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}