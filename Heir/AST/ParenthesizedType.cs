using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class ParenthesizedType(TypeRef type) : TypeRef
{
    public TypeRef Type { get; } = type;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitParenthesizedTypeRef(this);
    public override List<Token> GetTokens() => Type.GetTokens();
}