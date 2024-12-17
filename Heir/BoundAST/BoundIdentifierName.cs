using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundIdentifierName(Token token, BaseType type) : BoundName
    {
        public override BaseType Type => type;
        public Token Token { get; } = token;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundIdentifierNameExpression(this);
        public override List<Token> GetTokens() => [Token];

        public override void Display(int indent)
        {
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundIdentifierName({Token.Text})");
        }
    }
}
