using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class ArrayType(TypeRef elementType) : TypeRef
{
    public TypeRef ElementType { get; } = elementType;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitArrayTypeRef(this);
    public override List<Token> GetTokens() => ElementType.GetTokens();
}