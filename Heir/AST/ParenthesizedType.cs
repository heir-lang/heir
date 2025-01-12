using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ParenthesizedType(TypeRef type) : TypeRef
{
    public TypeRef Type { get; } = type;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitParenthesizedTypeRef(this);
    public override List<Token> GetTokens() => Type.GetTokens();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}ParenthesizedType(");
        Type.Display(indent + 1);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}