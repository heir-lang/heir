using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

// TODO: type params (ew)
public class FunctionType(Dictionary<string, TypeRef> parameterTypes, TypeRef returnType) : TypeRef
{
    public Dictionary<string, TypeRef> ParameterTypes { get; } = parameterTypes;
    public TypeRef ReturnType { get; } = returnType;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitFunctionTypeRef(this);

    public override List<Token> GetTokens() =>
    [
        ..ParameterTypes.Values.SelectMany(typeRef => typeRef.GetTokens()),
        ..ReturnType.GetTokens()
    ];
}