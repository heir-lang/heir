using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding
{
    public sealed class VariableSymbol(Token name, BaseType type, bool isMutable) : VariableSymbol<BaseType>(name, type, isMutable);

    public class VariableSymbol<T>(Token name, T type, bool isMutable) where T : BaseType
    {
        public Token Name { get; } = name;
        public T Type { get; } = type;
        public bool IsMutable { get; } = isMutable;

        public new string ToString() => $"{Name.Text}: {Type.ToString()}";
    }
}
