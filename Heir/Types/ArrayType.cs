namespace Heir.Types;

public class ArrayType(BaseType elementType) : BaseType
{
    public override TypeKind Kind => TypeKind.Singular;
    
    public BaseType ElementType { get; } = elementType;

    public override string ToString(bool colors = false) =>
        (ElementType is UnionType or IntersectionType
            ? '(' + ElementType.ToString() + ')'
            : ElementType.ToString(colors))
        + "[]";
}