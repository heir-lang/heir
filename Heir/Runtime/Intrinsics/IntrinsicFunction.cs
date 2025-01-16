using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public abstract class IntrinsicFunction<TDelegate>(string name, Dictionary<string, BaseType> parameterTypes, BaseType returnType, bool isGlobal)
    : IntrinsicValue(name, new FunctionType(parameterTypes, returnType), isGlobal)
    where TDelegate : Delegate
{
    public override IntrinsicFunction<TDelegate> Value => this;
    
    public abstract TDelegate Call { get; }
}