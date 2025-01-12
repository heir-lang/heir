using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Literal(Token token) : Expression
{
    public Token Token { get; } = token;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitLiteralExpression(this);
    public override List<Token> GetTokens() => [Token];

    public override void Display(int indent = 0)
    {
        var valueText = Token.Value?.ToString() ?? "none";
        if (Token.IsKind(SyntaxKind.BoolLiteral))
            valueText = valueText.ToLower();

        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}Literal({Token.Kind}, {valueText})");
    }
}