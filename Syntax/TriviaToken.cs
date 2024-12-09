namespace Heir.Syntax
{
    public class TriviaToken(TriviaKind kind, string text, Location startLocation, Location endLocation) : Token(SyntaxKind.Trivia, text, null, startLocation, endLocation)
    {
        public TriviaKind TriviaKind { get; } = kind;

        public override string ToString()
        {
            return $"{Kind} ({TriviaKind}): {(Value == null ? "null" : Value)} ({Text})    -> {StartLocation.ToString()} - {EndLocation.ToString()}";
        }
    }
}
