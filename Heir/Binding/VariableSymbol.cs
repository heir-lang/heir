using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding;

/// <summary>Binds a name to a type</summary>
public class VariableSymbol<T>(Token name, T type, bool isMutable, bool isIntrinsic = false) : ISymbol
    where T : BaseType
{
    public Token Name { get; } = name;
    public T Type { get; } = type;
    public bool IsMutable { get; } = isMutable;
    public bool IsIntrinsic { get; } = isIntrinsic;

    public new string ToString() => $"{(IsMutable ? "mut " : "")}{Name.Text}: {Type.ToString()}";
}