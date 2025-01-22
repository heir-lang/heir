using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public abstract class IntrinsicValue<TType>(string name, TType type, bool isGlobal) : IIntrinsicValue<TType>
    where TType : BaseType
{
    public string Name { get; } = name;
    public TType Type { get; } = type;
    BaseType IIntrinsicValue.Type => Type;
    public bool IsGlobal { get; } = isGlobal;
    
    public abstract object? Value { get; }
}