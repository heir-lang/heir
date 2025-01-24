using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundLiteral(Token token) : BoundExpression
{
    public override LiteralType Type => new(Token.Value);
    public Token Token { get; } = token;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundLiteralExpression(this);
    public override List<Token> GetTokens() => [Token];
}