using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

// TODO: type params (ew)
public sealed class FunctionType(Dictionary<string, TypeRef> parameterTypes, List<TypeParameter> typeParameters, TypeRef returnType) : TypeRef
{
    public Dictionary<string, TypeRef> ParameterTypes { get; } = parameterTypes;
    public List<TypeParameter> TypeParameters { get; } = typeParameters;
    public TypeRef ReturnType { get; } = returnType;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitFunctionTypeRef(this);

    public override List<Token> GetTokens() =>
    [
        ..ParameterTypes.Values.SelectMany(typeRef => typeRef.GetTokens()),
        ..ReturnType.GetTokens()
    ];
}