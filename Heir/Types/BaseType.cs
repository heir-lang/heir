using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.Types;

public abstract class BaseType
{
    public bool IsNullable => IsNone || (this is UnionType union && union.Types.Any(type => type.IsNone));
    public bool IsNone => this is PrimitiveType { PrimitiveKind: PrimitiveTypeKind.None };
    
    public abstract TypeKind Kind { get; }

    public abstract string ToString(bool colors = false);

    public static BaseType UnwrapParentheses(BaseType type)
    {
        while (true)
        {
            if (type is not ParenthesizedType parenthesizedType)
                return type;
            
            type = parenthesizedType.Type;
        }
    }

    public static BaseType Nullable(BaseType type)
    {
        return type.IsNullable || type is AnyType
            ? type
            : new UnionType([type, PrimitiveType.None]);
    }
    
    public static BaseType NonNullable(BaseType type)
    {
        if (!type.IsNullable || type is not UnionType union)
            return type;
        
        var nonNullableTypes = union.Types.FindAll(unionedType => !unionedType.IsNullable);
        return nonNullableTypes.Count == 1
            ? nonNullableTypes.First()
            : new UnionType(nonNullableTypes);
    }
    
    public static BaseType FromTypeRef(TypeRef typeRef)
    {
        return typeRef switch
        {
            AST.SingularType singularType =>
                singularType.Token?.IsKind(SyntaxKind.Identifier) ?? false
                    ? singularType.Token.Text == "any"
                        ? IntrinsicTypes.Any
                        : new SingularType(singularType.Token.Text)
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
        
        if (this is UnionType union)
            return union.Types.Any(type => type.IsAssignableTo(other));

        if (other is UnionType otherUnion)
            return otherUnion.Types.Any(IsAssignableTo);
        
        if (other is IntersectionType otherIntersection)
            return otherIntersection.Types.All(IsAssignableTo);
        
        if (this is IntersectionType intersection)
            return intersection.Types.Any(type => type.IsAssignableTo(other));
        
        if (this is ParenthesizedType parenthesized)
            return parenthesized.Type.IsAssignableTo(other);
        
        if (other is ParenthesizedType)
            return other.IsAssignableTo(this);
        
        if (this is ArrayType array)
            return other is ArrayType otherArray
                   && array.ElementType.IsAssignableTo(otherArray.ElementType);
        
        if (this is InterfaceType interfaceType && other is InterfaceType otherInterfaceType)
            return interfaceType.Members.Count == otherInterfaceType.Members.Count &&
                   interfaceType.IndexSignatures.Count == otherInterfaceType.IndexSignatures.Count &&
                   interfaceType.Members.All(member =>
                   {
                       var otherMember = otherInterfaceType.Members.GetValueOrDefault(member.Key);
                       return otherMember != null && member.Value.Type.IsAssignableTo(otherMember.Type); //&& member.Value.IsMutable == otherMember.IsMutable;
                   }) &&
                   interfaceType.IndexSignatures.All(indexSignature =>
                   {
                       var otherIndexSignature = otherInterfaceType.IndexSignatures.GetValueOrDefault(indexSignature.Key);
                       return otherIndexSignature != null && indexSignature.Value.IsAssignableTo(otherIndexSignature);
                   });

        if (other is InterfaceType)
            return false;
        
        if (this is FunctionType functionType)
        {
            var parameterIndex = 0;
            return other is FunctionType otherFunction &&
                   functionType.ReturnType.IsAssignableTo(otherFunction.ReturnType) &&
                   functionType.ParameterTypes.Count == otherFunction.ParameterTypes.Count &&
                   functionType.ParameterTypes.Values.All(parameterType =>
                       otherFunction.ParameterTypes.Values
                           .ElementAtOrDefault(parameterIndex++)?
                           .IsAssignableTo(parameterType) ?? false);
        }

        if (other is FunctionType)
            return other.IsAssignableTo(this);
        
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
        
        if (this is SingularType singular && other is SingularType otherSingular)
            return singular.Name == otherSingular.Name;

        return false;
    }
}