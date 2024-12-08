namespace Heir.Syntax
{
    public class TriviaToken(string text, Location location, TriviaKind kind) : Token(SyntaxKind.Trivia, text, null, location)
    {
        public TriviaKind TriviaKind { get; } = kind;
        public override string ToString()
        {
            return $"{Kind} ({TriviaKind}): {(Value == null ? "null" : Value)} ({Text})    -> {Location.ToString()}";
        }
    }
}
