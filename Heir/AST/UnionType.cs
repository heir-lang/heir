using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class UnionType(List<SingularType> types) : TypeRef
{
    public List<SingularType> Types { get; } = types;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnionTypeRef(this);
    public override List<Token> GetTokens() => Types.ConvertAll(type => type.Token);

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}UnionType(");
        foreach (var type in Types)
        {
            type.Display(indent + 1);
            Console.WriteLine(",");
        }
        Console.WriteLine(")");
    }
}