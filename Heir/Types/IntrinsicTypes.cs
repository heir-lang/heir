namespace Heir.Types;

public static class IntrinsicTypes
{
    public static readonly AnyType Any = new();
    public static readonly UnionType Index = new([
        PrimitiveType.Int,
        PrimitiveType.String
    ]);
    public static readonly UnionType Number = new([
        PrimitiveType.Int,
        PrimitiveType.Float
    ]);
    public static readonly UnionType StringOrChar = new([
        PrimitiveType.String,
        PrimitiveType.Char
    ]);
}