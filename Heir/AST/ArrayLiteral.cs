using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class ArrayLiteral(Token token, List<Expression>? elements = null) : Literal(token)
{
    public List<Expression> Elements { get; } = elements ?? [];

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitArrayLiteralExpression(this);
    public override List<Token> GetTokens() => [Token, ..Elements.SelectMany(e => e.GetTokens())];
}