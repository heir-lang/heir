namespace Heir.Runtime.HookedExceptions;

public class ReturnHook(object? value) : HookedException
{
    public object? Value { get; } = value;
}

public abstract class HookedException() : Exception("BUG: Unhooked hooked exception");