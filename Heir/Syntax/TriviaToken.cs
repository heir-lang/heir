namespace Heir.Syntax;

public class TriviaToken(TriviaKind kind, string text, Location startLocation, Location endLocation) : Token(SyntaxKind.Trivia, text, null, startLocation, endLocation)
{
    public TriviaKind TriviaKind { get; } = kind;

    public override string ToString() =>
        $"{Kind} ({TriviaKind}): {(Value == null ? "" : '(' + Value.ToString() + ')')} {Utility.EscapeTabsAndNewlines(Text)}        {StartLocation} - {EndLocation}";
}