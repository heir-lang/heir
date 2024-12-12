using Heir.Syntax;

namespace Heir.AST
{
    public class IdentifierName(Token token) : Name
    {
        public Token Token { get; } = token;

        public override List<Token> GetTokens() => [Token];

        public override void Display(int indent)
        {
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}IdentifierName({Token.Text})");
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
