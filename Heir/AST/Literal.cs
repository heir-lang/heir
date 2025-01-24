using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Literal(Token token) : Expression
{
    public Token Token { get; } = token;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitLiteralExpression(this);
    public override List<Token> GetTokens() => [Token];
}