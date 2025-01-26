namespace Heir.Runtime;

public class BreakHookException : HookedException;
public class NextHookException : HookedException;

public abstract class HookedException() : Exception("BUG: Unhooked hooked exception");