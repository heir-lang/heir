using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding;

public class VariableSymbol<T>(Token name, T type, bool isMutable, bool isIntrinsic = false) where T : BaseType
{
    public Token Name { get; } = name;
    public T Type { get; } = type;
    public bool IsMutable { get; } = isMutable;
    public bool IsIntrinsic { get; } = isIntrinsic;

    public new string ToString() => $"{Name.Text}: {Type.ToString()}";
}