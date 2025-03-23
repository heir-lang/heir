namespace Heir.Types;

public sealed class UnionType(List<BaseType> types) : BaseType
{
    public override TypeKind Kind => TypeKind.Union;
    public List<BaseType> Types { get; } = types.Distinct().ToList();
    
    public override string ToString(bool colors = false)
    {
        var filteredTypes = Types
            .FindAll(type => !type.IsNone)
            .ConvertAll(t => t is FunctionType ? '(' + t.ToString(colors) + ')' : t.ToString(colors));

        var addParentheses = IsNullable && filteredTypes.Count > 1;
        return (addParentheses ? "(" : "") +
               string.Join(" | ", filteredTypes) +
               (addParentheses ? ")" : "") +
               (IsNullable ? "?" : "");
    }
}