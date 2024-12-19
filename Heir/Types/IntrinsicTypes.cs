namespace Heir.Types
{
    public static class IntrinsicTypes
    {
        public static AnyType Any = new();
        public static UnionType Index = new UnionType([
            PrimitiveType.Int,
            PrimitiveType.String
        ]);
        public static UnionType Number = new UnionType([
            PrimitiveType.Int,
            PrimitiveType.Float
        ]);
        public static UnionType StringOrChar = new UnionType([
            PrimitiveType.String,
            PrimitiveType.Char
        ]);
    }
}
