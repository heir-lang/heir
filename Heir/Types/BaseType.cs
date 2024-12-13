namespace Heir.Types
{
    public abstract class BaseType
    {
        public abstract TypeKind Kind { get; }

        public abstract string ToString(bool colors = false);

        public bool IsAssignableTo(BaseType other)
        {
            if (this is UnionType union)
                return union.Types.Any(type => type.IsAssignableTo(other));
            else if (other is UnionType)
                return other.IsAssignableTo(this);
            else if (this is SingularType singular && other is SingularType otherSingular)
                return singular.Name == otherSingular.Name;

            return Kind == other.Kind; // temp
        }
    }
}
