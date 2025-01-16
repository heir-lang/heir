namespace Heir.Types;

public sealed class UnionType(List<BaseType> types) : BaseType
{
    public override TypeKind Kind => TypeKind.Union;
    public List<BaseType> Types { get; } = types;

    public override string ToString(bool colors = false)
    {
        var anyNullable = Types.Any(type => type.IsNone());
        var filteredTypes = new HashSet<string>(Types
                .FindAll(type => !type.IsNone())
                .ConvertAll(t => t.ToString(colors)));

        var addParentheses = anyNullable && filteredTypes.Count > 0;
        return (addParentheses ? "(" : "") +
               string.Join(" | ", filteredTypes) +
               (addParentheses ? ")" : "") +
               (anyNullable ? "?" : "");
    }
}