using Heir.AST;
using Heir.CodeGeneration;

namespace Heir.Runtime.Values;

public sealed class FunctionValue(List<Instruction> bodyBytecode, Scope closure)
{
    public Guid ID { get; } = Guid.NewGuid();
    public List<Instruction> BodyBytecode { get; } = bodyBytecode;
    public Scope Closure { get; } = closure;

    public override string ToString() => $"<function: {ID}>";
}