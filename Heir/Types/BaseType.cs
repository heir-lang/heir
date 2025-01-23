using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.Types;

public abstract class BaseType
{
    public bool IsNullable => IsNone || (this is UnionType union && union.Types.Any(type => type.IsNone));
    public bool IsNone => this is PrimitiveType { PrimitiveKind: PrimitiveTypeKind.None };
    
    public abstract TypeKind Kind { get; }

    public abstract string ToString(bool colors = false);

    public static BaseType Nullable(BaseType type)
    {
        return type.IsNullable || type is AnyType
            ? type
            : new UnionType([type, PrimitiveType.None]);
    }
    
    public static BaseType FromTypeRef(TypeRef typeRef)
    {
        return typeRef switch
        {
            AST.SingularType singularType =>
                singularType.Token?.IsKind(SyntaxKind.Identifier) ?? false
                    ? new SingularType(singularType.Token.Text)
                    : singularType.Token != null
                        ? SyntaxFacts.PrimitiveTypeMap.GetValueOrDefault(singularType.Token.Kind) ?? PrimitiveType.None
                        : PrimitiveType.None,
            
            AST.ParenthesizedType parenthesizedType =>
                new ParenthesizedType(FromTypeRef(parenthesizedType.Type)),
            
            AST.UnionType unionType =>
                new UnionType(unionType.Types.ConvertAll(FromTypeRef)),
            
            AST.IntersectionType intersectionType =>
                new IntersectionType(intersectionType.Types.ConvertAll(FromTypeRef)),
            
            AST.FunctionType functionType =>
                new FunctionType([], 
                    functionType.ParameterTypes
                        .Select(pair => new KeyValuePair<string, BaseType>(pair.Key, FromTypeRef(pair.Value)))
                        .ToDictionary(),
                    FromTypeRef(functionType.ReturnType)),
            
            _ => PrimitiveType.None
        };
    }

    public bool IsAssignableTo(BaseType other)
    {
        if (this is AnyType || other is AnyType)
            return true;

        if (this is FunctionType functionType)
        {
            var parameterIndex = 0;
            return other is FunctionType otherFunction &&
                   functionType.ReturnType.IsAssignableTo(otherFunction.ReturnType) &&
                   functionType.ParameterTypes.Values.All(parameterType =>
                       otherFunction.ParameterTypes.Values
                           .ElementAtOrDefault(parameterIndex++)?
                           .IsAssignableTo(parameterType) ?? false);
        }

        if (other is FunctionType)
            return other.IsAssignableTo(this);
        
        if (this is UnionType union)
            return union.Types.Any(type => type.IsAssignableTo(other));

        if (other is UnionType otherUnion)
            return otherUnion.Types.Any(IsAssignableTo);
        
        if (other is IntersectionType otherIntersection)
            return otherIntersection.Types.All(IsAssignableTo);
        
        if (this is IntersectionType intersection)
            return intersection.Types.Any(type => type.IsAssignableTo(other));
        
        if (this is LiteralType literalType)
        {
            if (other is LiteralType otherLiteralType)
                return literalType.Equals(otherLiteralType);
            
            if (other is ParenthesizedType otherParenthesized)
                return IsAssignableTo(otherParenthesized.Type);

            var primitiveType = PrimitiveType.FromValue(literalType.Value);
            if (primitiveType != null && other is PrimitiveType otherPrimitiveType)
                return primitiveType.IsAssignableTo(otherPrimitiveType);
        }
        
        if (other is LiteralType)
            return false;
        
        if (this is ParenthesizedType parenthesized)
            return parenthesized.Type.IsAssignableTo(other);
        
        if (other is ParenthesizedType)
            return other.IsAssignableTo(this);
        
        if (this is SingularType singular && other is SingularType otherSingular)
            return singular.Name == otherSingular.Name;

        return false;
    }
}