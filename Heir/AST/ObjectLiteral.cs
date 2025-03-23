using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class ObjectLiteral(Token token, Dictionary<Expression, Expression>? properties = null) : Literal(token)
{
    public Dictionary<Expression, Expression> Properties { get; } = properties ?? [];

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitObjectLiteralExpression(this);
    public override List<Token> GetTokens() =>
    [
        Token,
        ..Properties.SelectMany(pair => new List<Token>([..pair.Key.GetTokens(), ..pair.Value.GetTokens()]))
    ];
}