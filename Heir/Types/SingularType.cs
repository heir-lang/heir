namespace Heir.Types
{
    public class SingularType(string name, BaseType[]? typeArguments = null) : BaseType
    {
        public override TypeKind Kind => TypeKind.Singular;
        public string Name { get; } = name;
        public BaseType[]? TypeArguments { get; } = typeArguments;

        public static SingularType FromValue(object? value)
        {
            switch (value)
            {
                case long:
                    return new SingularType("int");
                case double:
                    return new SingularType("float");
                case bool:
                    return new SingularType("bool");

                default:
                    {
                        // handle array/range/dict types

                        return new SingularType(value!.GetType().Name);
                    }
            }
        }

        public static SingularType FromLiteral(LiteralType literal)
        {
            return FromValue(literal.Value);
        }

        public new string ToString(bool colors = false)
        {
            return Name + (TypeArguments != null ? $"<{string.Join(", ", TypeArguments.Select(t => t.ToString(colors)))}>" : "");
        }
    }
}
