namespace Heir.Types;

// TODO: type params (ew)
public sealed class FunctionType(
    Dictionary<string, object?> defaults,
    Dictionary<string, BaseType> parameters,
    List<TypeParameter> typeParameters,
    BaseType returnType
) : SingularType("Function")
{
    public override TypeKind Kind => TypeKind.Function;
    
    public Dictionary<string, object?> Defaults { get; } = defaults;
    public Dictionary<string, BaseType> Parameters { get; } = parameters;
    public List<TypeParameter> TypeParameters { get; } = typeParameters;
    public BaseType ReturnType { get; } = returnType;
    public Range Arity
    {
        get
        {
            var parameters = Parameters.Values.ToList();
            var nullableParameters = Parameters.ToList()
                .FindAll(pair => pair.Value.IsNullable || Defaults.GetValueOrDefault(pair.Key) != null);
            
            var nonNullableParametersCount = parameters.Count - nullableParameters.Count;
            return nonNullableParametersCount..parameters.Count;
        }
    }

    public override string ToString(bool colors = false)
    {
        var parameterList = string.Join(", ", Parameters.Select(pair => $"{pair.Key}: {pair.Value.ToString()}"));
        return $"{(TypeParameters.Count > 0 ? "<" + string.Join(", ", TypeParameters.ConvertAll(p => p.ToString(colors))) + ">" : "")}({parameterList}) -> {ReturnType.ToString(colors)}";
    }
}