namespace Heir.Types
{
    public class LiteralType(object? value) : SingularType(value?.ToString() ?? "none")
    {
        public override TypeKind Kind => TypeKind.Literal;
        public object? Value { get; } = value;
    }
}
