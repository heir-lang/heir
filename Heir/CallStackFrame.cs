using Heir.CodeGeneration;
using Heir.Runtime;

namespace Heir;

internal sealed class CallStackFrame(Bytecode bytecode, Scope closure, int enclosingPointer)
{
    public Bytecode Bytecode { get; } = bytecode;
    public Scope Closure { get; } = closure;
    public int EnclosingPointer { get; } = enclosingPointer;
}