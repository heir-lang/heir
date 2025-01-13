namespace Heir.Runtime.HookedExceptions;

public class Return<TValue>(TValue value) : HookedException
{
    public TValue Value { get; } = value;
}

public abstract class HookedException() : Exception("BUG: Unhooked hooked exception");