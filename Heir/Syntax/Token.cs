namespace Heir.Syntax;

public class Token
{
    public SyntaxKind Kind { get; }
    public string Text { get; }
    public object? Value { get; }
    public Span Span { get; }
    
    public Token(SyntaxKind syntax, string text, object? value, Location start, Location end)
    {
        Kind = syntax;
        Text = text;
        Value = value;
        Span = new Span(start, end);
    }

    public Token(SyntaxKind syntax, string text, object? value, Span span)
    {
        Kind = syntax;
        Text = text;
        Value = value;
        Span = span;
    }

    public Token WithKind(SyntaxKind kind) => new(kind, Text, Value, Span);
    public bool IsKind(SyntaxKind kind) => Kind == kind;
    public override string ToString() =>
        $"{Kind}: {Text} {(Value == null ? "" : '(' + Value.ToString() + ')')}      {Span}";
}