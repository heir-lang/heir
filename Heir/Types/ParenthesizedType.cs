namespace Heir.Types;

public class ParenthesizedType(BaseType type) : BaseType
{
    public override TypeKind Kind => TypeKind.Singular;
    
    public BaseType Type { get; } = type;

    public override string ToString(bool colors = false) => "(" + Type.ToString(colors) + ")";
}