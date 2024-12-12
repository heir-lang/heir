namespace Heir.Types
{
    public static class IntrinsicTypes
    {
        public static UnionType Number = new UnionType([
            new PrimitiveType(PrimitiveTypeKind.Int),
            new PrimitiveType(PrimitiveTypeKind.Float)
        ]);
    }
}
