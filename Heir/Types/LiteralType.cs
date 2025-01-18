namespace Heir.Types;

public class LiteralType(object? value) : PrimitiveType(
    value switch
    {
        string => PrimitiveTypeKind.String,
        char => PrimitiveTypeKind.Char,
        long or ulong or int or uint or short or ushort or byte or sbyte => PrimitiveTypeKind.Int,
        double or float or decimal => PrimitiveTypeKind.Float,
        bool => PrimitiveTypeKind.Bool,
        _ => PrimitiveTypeKind.None
    })
{
    public override TypeKind Kind => TypeKind.Literal;
    public object? Value { get; } = value;
    
    public PrimitiveType AsPrimitive() => FromValue(Value) ?? None;

    public override bool Equals(object? obj) => obj is LiteralType otherLiteralType &&
                                                EqualityComparer<object?>.Default.Equals(Value, otherLiteralType.Value);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public override string ToString(bool colors = false) => Utility.Repr(Value, colors);
}