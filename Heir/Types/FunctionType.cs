namespace Heir.Types;

// TODO: type params (ew)
public sealed class FunctionType(Dictionary<string, BaseType> parameterTypes, BaseType returnType)
    : SingularType("Function")
{
    public override TypeKind Kind => TypeKind.Function;

    public override string ToString(bool colors = false)
    {
        var parameterList = string.Join(", ", parameterTypes.Select(pair => $"{pair.Value}: {pair.Key}"));
        return $"({parameterList}) -> {returnType.ToString(colors)}";
    }
}