namespace Heir.Types;

public class LiteralType(object? value) : BaseType
{
    public override TypeKind Kind => TypeKind.Literal;
    public object? Value { get; } = value;

    // TODO: format so that strings are surrounded in quotes, etc. repr type shit
    public override string ToString(bool colors = false) => Value?.ToString() ?? "none";
}