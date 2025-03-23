using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class TypeParameter(IdentifierName name, TypeRef? baseType, TypeRef? initializer) : TypeRef
{
    public IdentifierName Name { get; } = name;
    public TypeRef? BaseType { get; } = baseType;
    public TypeRef? Initializer { get; } = initializer;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitTypeParameter(this);
    public override List<Token> GetTokens() => Name.GetTokens().Concat(Initializer?.GetTokens() ?? []).ToList();
    
    public TypeParameter WithName(IdentifierName name) => new(name, BaseType, Initializer);
    public TypeParameter WithBaseType(TypeRef? baseType) => new(Name, baseType, Initializer);
    public TypeParameter WithInitializer(TypeRef? initializer) => new(Name, BaseType, initializer);
}