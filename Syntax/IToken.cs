namespace Heir.Syntax
{
    public interface IToken
    {
        SyntaxKind Syntax { get; }
        object Value { get; }
        string Text { get; }
        Location Location { get; }
    }
}
