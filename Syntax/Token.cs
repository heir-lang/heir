namespace Heir.Syntax
{
    public class Token(SyntaxKind syntax, string text, object value, Location location)
    {
        SyntaxKind Syntax { get; } = syntax;
        string Text { get; } = text;
        object? Value { get; } = value;
        Location Location { get; } = location;
    }
}
