using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding;

/// <summary>Binds a type to a name</summary>
public class TypeSymbol(Token name, BaseType type, bool isIntrinsic = false) : ISymbol
{
    public Token Name { get; } = name;
    public BaseType Type { get; } = type;
    public bool IsIntrinsic { get; } = isIntrinsic;

    public bool IsAssignableTo(TypeSymbol other) =>
        Type.IsAssignableTo(other.Type);
}