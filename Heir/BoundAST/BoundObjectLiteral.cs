using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundObjectLiteral(Token token, Dictionary<BaseType, BoundExpression> properties, InterfaceType type) : BoundExpression
{
    public Token Token { get; } = token;
    public override InterfaceType Type => type;
    public Dictionary<BaseType, BoundExpression> Properties { get; } = properties;

    public override List<Token> GetTokens() => [Token, ..Properties.Values.SelectMany(expr => expr.GetTokens())];
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundObjectLiteralExpression(this);
}