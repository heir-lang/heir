namespace Heir.Types;

// TODO: type params (ew)
public sealed class FunctionType(Dictionary<string, BaseType> parameterTypes, BaseType returnType)
    : SingularType("Function")
{
    public override TypeKind Kind => TypeKind.Function;
    
    public Dictionary<string, BaseType> ParameterTypes { get; } = parameterTypes;
    public BaseType ReturnType { get; } = returnType;

    public override string ToString(bool colors = false)
    {
        var parameterList = string.Join(", ", ParameterTypes.Select(pair => $"{pair.Value}: {pair.Key}"));
        return $"({parameterList}) -> {ReturnType.ToString(colors)}";
    }
}