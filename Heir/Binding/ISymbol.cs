using Heir.Syntax;
using Heir.Types;

namespace Heir.Binding;

public interface ISymbol
{
    public Token Name { get; }
    public bool IsIntrinsic { get; }
}