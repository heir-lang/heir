using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundWhile(Token keyword, BoundExpression condition, BoundStatement body) : BoundStatement
{
    public override BaseType? Type => null;
    
    public Token Keyword { get; } = keyword;
    public BoundExpression Condition { get; } = condition;
    public BoundStatement Body { get; } = body;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundWhileStatement(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        ..Condition.GetTokens(),
        ..Body.GetTokens()
    ];
}