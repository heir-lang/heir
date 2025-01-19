using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public abstract class IntrinsicFunction(string name, Dictionary<string, BaseType> parameterTypes, BaseType returnType, bool isGlobal)
    : IntrinsicValue(name, new FunctionType([], parameterTypes, returnType), isGlobal)
{
    public override IntrinsicFunction Value => this;

    public delegate object? BaseDelegate(List<object?> args);
    public abstract BaseDelegate Invoke { get; }
}