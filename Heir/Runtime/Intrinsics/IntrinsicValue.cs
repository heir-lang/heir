using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public abstract class IntrinsicValue(string name, BaseType type, bool isGlobal) : IValue
{
    public string Name { get; } = name;
    public BaseType Type { get; } = type;
    public bool IsGlobal { get; } = isGlobal;
    
    public abstract object? Value { get; }
}