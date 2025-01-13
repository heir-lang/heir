namespace Heir.Runtime.HookedExceptions;

public class BreakHook : HookedException;
public class NextHook : HookedException;

public abstract class HookedException() : Exception("BUG: Unhooked hooked exception");