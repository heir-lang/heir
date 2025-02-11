using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public abstract class IntrinsicFunction(
    string name,
    Dictionary<string, BaseType> parameterTypes,
    BaseType returnType,
    bool isGlobal = false) : IntrinsicValue<FunctionType>(name, new([], parameterTypes, returnType), isGlobal)
{
    public override IntrinsicFunction Value => this;
    
    public int Arity { get; } = parameterTypes.Count;
    public delegate object? BaseDelegate(List<object?> args);
    public abstract BaseDelegate Invoke { get; }
}