using Heir.Syntax;
using Heir.Types;

namespace Heir
{
    public class VariableSymbol(Token name, BaseType type) : VariableSymbol<BaseType>(name, type);

    public class VariableSymbol<T>(Token name, T type) where T : BaseType
    {
        public Token Name { get; } = name;
        public T Type { get; } = type;

        public new string ToString() => $"{Type.ToString()} {Name.Text}";
    }
}
