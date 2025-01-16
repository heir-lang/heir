namespace Heir.Types;

public sealed class UnionType(List<BaseType> types) : BaseType
{
    public override TypeKind Kind => TypeKind.Union;
    public List<BaseType> Types { get; } = types;
    
    public override string ToString(bool colors = false)
    {
        var filteredTypes = Types
            .Distinct()
            .ToList()
            .FindAll(type => !type.IsNone)
            .ConvertAll(t => t.ToString(colors));

        var addParentheses = IsNullable && filteredTypes.Count > 1;
        return (addParentheses ? "(" : "") +
               string.Join(" | ", filteredTypes) +
               (addParentheses ? ")" : "") +
               (IsNullable ? "?" : "");
    }
}