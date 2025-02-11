using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class While(Token keyword, Expression condition, Statement body) : Statement
{
    public Token Keyword { get; } = keyword;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitWhileStatement(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        ..Condition.GetTokens(),
        ..Body.GetTokens()
    ];
}