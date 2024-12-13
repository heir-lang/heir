namespace Heir.Types
{
    public static class IntrinsicTypes
    {
        public static UnionType Number = new UnionType([
            PrimitiveType.Int,
            PrimitiveType.Float
        ]);
    }
}
