namespace Heir.Types
{
    enum PrimitiveTypeKind
    {
        Int,
        Float,
        String,
        Char,
        Bool,
        None
    }

    public class PrimitiveType : SingularType
    {
        public static PrimitiveType Int = new PrimitiveType(PrimitiveTypeKind.Int);
        public static PrimitiveType Float = new PrimitiveType(PrimitiveTypeKind.Float);
        public static PrimitiveType String = new PrimitiveType(PrimitiveTypeKind.String);
        public static PrimitiveType Char = new PrimitiveType(PrimitiveTypeKind.Char);
        public static PrimitiveType Bool = new PrimitiveType(PrimitiveTypeKind.Bool);
        public static PrimitiveType None = new PrimitiveType(PrimitiveTypeKind.None);

        public override TypeKind Kind => TypeKind.Primitive;

        private PrimitiveType(PrimitiveTypeKind primitiveTypeKind)
            : base(primitiveTypeKind.ToString().ToLower())
        { 
        }

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
