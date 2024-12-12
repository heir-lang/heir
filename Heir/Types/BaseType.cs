namespace Heir.Types
{
    public abstract class BaseType
    {
        public abstract TypeKind Kind { get; }

        public bool IsAssignableTo(BaseType other)
        {
            return Kind == other.Kind; // temp
        }

        public string ToString(bool colors = false)
        {
            return "";
        }
    }
}
