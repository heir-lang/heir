using Heir.AST.Abstract;

namespace Heir;

public sealed class StackFrame(SyntaxNode? node, object? value)
{
    public static readonly StackFrame ExitMarker = new(null, new ExitMarker());
    
    public SyntaxNode? Node { get; } = node;
    public object? Value { get; } = value;
}