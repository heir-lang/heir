namespace Heir.Types
{
    public class SingularType(string name, BaseType[]? typeArguments = null) : BaseType
    {
        public override TypeKind Kind => TypeKind.Singular;
        public string Name { get; } = name;
        public BaseType[]? TypeArguments { get; } = typeArguments;

        public new string ToString(bool colors = false)
        {
            return Name + (TypeArguments != null ? $"<{string.Join(", ", TypeArguments.Select(t => t.ToString(colors)))}>" : "");
        }
    }
}
