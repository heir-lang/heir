using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class IntersectionType(List<TypeRef> types) : TypeRef
{
    public List<TypeRef> Types { get; } = types;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitIntersectionTypeRef(this);
    public override List<Token> GetTokens() => Types.SelectMany(type => type.GetTokens()).ToList();
}