namespace Heir.Types
{
    public class AnyType() : SingularType("any")
    {
        public override TypeKind Kind => TypeKind.Any;
    }
}
