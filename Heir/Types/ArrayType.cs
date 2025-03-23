namespace Heir.Types;

public class ArrayType(BaseType elementType)
    : InterfaceType(
        [],
        new Dictionary<PrimitiveType, BaseType>([new(PrimitiveType.Int, elementType)]),
        $"Array<{elementType.ToString()}>"
    )
{
    public override TypeKind Kind => TypeKind.Singular;
    
    public BaseType ElementType { get; } = elementType;

    public override string ToString(bool colors = false) =>
        (ElementType is UnionType or IntersectionType
            ? '(' + ElementType.ToString() + ')'
            : ElementType.ToString(colors))
        + "[]";
}