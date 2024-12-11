namespace Heir.Types
{
    public abstract class BaseType
    {
        public abstract TypeKind Kind { get; }

        public string ToString(bool colors = false)
        {
            return "";
        }
    }
}
