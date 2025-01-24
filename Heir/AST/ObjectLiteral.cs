using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ObjectLiteral(Token token, Dictionary<Expression, Expression> properties) : Literal(token)
{
    public Dictionary<Expression, Expression> Properties { get; } = properties;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitObjectLiteralExpression(this);
}