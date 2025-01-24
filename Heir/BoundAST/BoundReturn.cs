using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundReturn(Token keyword, BoundExpression expression) : BoundStatement
{
    public override BaseType Type => Expression.Type;

    public Token Keyword { get; } = keyword;
    public BoundExpression Expression { get; } = expression;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundReturnStatement(this);
    public override List<Token> GetTokens() => [];
}