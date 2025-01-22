using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Return(Token keyword, Expression expression) : Statement
{
    public Token Keyword { get; } = keyword;
    public Expression Expression { get; } = expression;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitReturnStatement(this);
    public override List<Token> GetTokens() => [Keyword, ..Expression.GetTokens()];
}