using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class UnionType(List<TypeRef> types) : TypeRef
{
    public List<TypeRef> Types { get; } = types;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnionTypeRef(this);
    public override List<Token> GetTokens() => Types.SelectMany(type => type.GetTokens()).ToList();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}UnionType(");
        foreach (var type in Types)
        {
            type.Display(indent + 1);
            Console.WriteLine(",");
        }
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}