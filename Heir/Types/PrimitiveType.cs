namespace Heir.Types
{
    public enum PrimitiveTypeKind
    {
        Int,
        Float,
        String,
        Char,
        Bool,
        None
    }

    public class PrimitiveType(PrimitiveTypeKind primitiveTypeKind) : SingularType(primitiveTypeKind.ToString().ToLower())
    {
        public override TypeKind Kind => TypeKind.Primitive;

        public static PrimitiveType FromValue(object? value)
        {
            // TODO: do something with _ or "not null" patterns
            var primitiveTypeKind = value switch
            {
                long => PrimitiveTypeKind.Int,
                double => PrimitiveTypeKind.Float,
                string => PrimitiveTypeKind.String,
                char => PrimitiveTypeKind.Char,
                bool => PrimitiveTypeKind.Bool,
                null => PrimitiveTypeKind.None
            };

            return new(primitiveTypeKind);
        }
    }
}
