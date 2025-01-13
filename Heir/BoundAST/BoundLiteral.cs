using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundLiteral(Token token) : BoundExpression
    {
        public override PrimitiveType Type => PrimitiveType.FromValue(Token.Value);
        public Token Token { get; } = token;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundLiteralExpression(this);
        public override List<Token> GetTokens() => [Token];

        public override void Display(int indent = 0)
        {
            var valueText = Token.Value?.ToString() ?? "none";
            if (Token.IsKind(SyntaxKind.BoolLiteral))
                valueText = valueText.ToLower();

            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundLiteral({Token.Kind}, {valueText})");
        }
    }
}
