using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class UnionType(List<TypeRef> types) : TypeRef
{
    public List<TypeRef> Types { get; } = types;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnionTypeRef(this);
    public override List<Token> GetTokens() => Types.SelectMany(type => type.GetTokens()).ToList();
}