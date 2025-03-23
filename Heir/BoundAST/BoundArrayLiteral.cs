using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundArrayLiteral(Token bracket, List<BoundExpression> elements, ArrayType type) : BoundExpression
{
    public Token Token { get; } = bracket;
    public override ArrayType Type => type;
    public List<BoundExpression> Elements { get; } = elements;

    public override List<Token> GetTokens() => [Token, ..Elements.SelectMany(expr => expr.GetTokens())];
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundArrayLiteralExpression(this);
}