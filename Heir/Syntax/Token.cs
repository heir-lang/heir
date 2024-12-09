namespace Heir.Syntax
{
    public class Token(SyntaxKind syntax, string text, object? value, Location startLocation, Location endLocation)
    {
        public SyntaxKind Kind { get; } = syntax;
        public string Text { get; } = text;
        public object? Value { get; } = value;
        public Location StartLocation { get; } = startLocation;
        public Location EndLocation { get; } = endLocation;

        public bool IsKind(SyntaxKind kind)
        {
            return Kind == kind;
        }

        public override string ToString()
        {
            return $"{Kind}: {(Value == null ? "null" : Value.ToString())} ({Text})    -> {StartLocation.ToString()} - {EndLocation.ToString()}";
        }
    }
}
