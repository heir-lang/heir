namespace Heir.Syntax
{
    public class Token(SyntaxKind syntax, string text, object value, Location location)
    {
        public SyntaxKind Kind { get; } = syntax;
        public string Text { get; } = text;
        public object? Value { get; } = value;
        public Location Location { get; } = location;

        public bool IsKind(SyntaxKind kind)
        {
            return Kind == kind;
        }
    }
}
