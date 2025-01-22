using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundIf(Token keyword, BoundExpression condition, BoundStatement body, BoundStatement? elseBranch)
    : BoundStatement
{
    public override BaseType? Type => null;
    
    public Token Keyword { get; } = keyword;
    public BoundExpression Condition { get; } = condition;
    public BoundStatement Body { get; } = body;
    public BoundStatement? ElseBranch { get; } = elseBranch;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundIfStatement(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        ..Condition.GetTokens(),
        ..Body.GetTokens(),
        ..ElseBranch == null ? [] : ElseBranch.GetTokens()
    ];
}