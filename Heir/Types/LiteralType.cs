namespace Heir.Types;

public class LiteralType(object? value) : BaseType
{
    public override TypeKind Kind => TypeKind.Literal;
    public object? Value { get; } = value;

    public override string ToString(bool colors = false) => Utility.Repr(Value, colors);
}