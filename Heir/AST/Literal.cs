using Heir.Syntax;

namespace Heir.AST
{
    public class Literal(Token token) : Expression
    {
        public Token Token { get; } = token;

        public override void Display(int indent)
        {
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}Literal({Token.Kind}, {Token.Value?.ToString()})");
        }
    }
}
