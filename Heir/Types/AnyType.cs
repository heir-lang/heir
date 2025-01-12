namespace Heir.Types;

public sealed class AnyType() : SingularType("any")
{
    public override TypeKind Kind => TypeKind.Any;
}