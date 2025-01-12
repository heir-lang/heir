using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.Types;

public abstract class BaseType
{
    public abstract TypeKind Kind { get; }

    public abstract string ToString(bool colors = false);

    public static BaseType FromTypeRef(TypeRef typeRef)
    {
        switch (typeRef)
        {
            case AST.SingularType singularType:
                return singularType.Token.IsKind(SyntaxKind.Identifier)
                    ? new SingularType(singularType.Token.Text)
                    : SyntaxFacts.PrimitiveTypeMap[singularType.Token.Kind];
            case AST.UnionType unionType:
                return new UnionType(unionType.Types.ConvertAll(FromTypeRef));
            case AST.IntersectionType intersectionType:
                return new IntersectionType(intersectionType.Types.ConvertAll(FromTypeRef));
        }

        return PrimitiveType.None;
    }

    public bool IsAssignableTo(BaseType other)
    {
        if (this is AnyType || other is AnyType)
            return true;
        
        if (this is UnionType union)
            return union.Types.Any(type => type.IsAssignableTo(other));
        
        if (other is UnionType)
            return other.IsAssignableTo(this);
        
        if (this is IntersectionType intersection)
            return intersection.Types.All(type => type.IsAssignableTo(other));
        
        if (other is IntersectionType)
            return other.IsAssignableTo(this);
        
        if (this is LiteralType literal && other is LiteralType otherLiteral)
            return literal.Value == otherLiteral.Value;
        
        if (this is SingularType singular && other is SingularType otherSingular)
            return singular.Name == otherSingular.Name;

        return false;
    }
}