using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class IdentifierName(Token token) : Name
{
    public Token Token { get; } = token;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitIdentifierNameExpression(this);
    public override List<Token> GetTokens() => [Token];

    public override void Display(int indent = 0)
    {
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}IdentifierName({Token.Text})");
    }

    public override string ToString() => Token.Text;
}