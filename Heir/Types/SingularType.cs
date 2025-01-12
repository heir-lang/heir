namespace Heir.Types;

public class SingularType(string name, List<BaseType>? typeArguments = null) : BaseType
{
    public override TypeKind Kind => TypeKind.Singular;
    public string Name { get; } = name;
    public List<BaseType>? TypeArguments { get; } = typeArguments;

    public override string ToString(bool colors = false) =>
        Name + (TypeArguments != null ? $"<{string.Join(", ", TypeArguments.Select(t => t.ToString(colors)))}>" : "");
}