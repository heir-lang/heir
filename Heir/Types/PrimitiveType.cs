namespace Heir.Types;

public enum PrimitiveTypeKind
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
    public static readonly PrimitiveType Int = new(PrimitiveTypeKind.Int);
    public static readonly PrimitiveType Float = new(PrimitiveTypeKind.Float);
    public static readonly PrimitiveType String = new(PrimitiveTypeKind.String);
    public static readonly PrimitiveType Char = new(PrimitiveTypeKind.Char);
    public static readonly PrimitiveType Bool = new(PrimitiveTypeKind.Bool);
    public static readonly PrimitiveType None = new(PrimitiveTypeKind.None);

    public override TypeKind Kind => TypeKind.Primitive;

    public PrimitiveTypeKind PrimitiveKind { get; }

    protected PrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        : base(primitiveTypeKind.ToString().ToLower())
    {
        PrimitiveKind = primitiveTypeKind;
    }

    public static PrimitiveType FromValue(object? value)
    {
        var primitiveTypeKind = value switch
        {
            string => PrimitiveTypeKind.String,
            char => PrimitiveTypeKind.Char,
            long or ulong or int or uint or short or ushort or byte or sbyte => PrimitiveTypeKind.Int,
            double or float or decimal => PrimitiveTypeKind.Float,
            bool => PrimitiveTypeKind.Bool,
            _ => PrimitiveTypeKind.None
        };

        return new PrimitiveType(primitiveTypeKind);
    }
}