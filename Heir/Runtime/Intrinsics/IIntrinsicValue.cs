using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public interface IIntrinsicValue<out TType> : IIntrinsicValue
    where TType : BaseType
{
    public new TType Type { get; }
}

public interface IIntrinsicValue : IValue
{
    public string Name { get; }
    public BaseType Type { get; }
    public bool IsGlobal { get; }
    public object? Value { get; }
}