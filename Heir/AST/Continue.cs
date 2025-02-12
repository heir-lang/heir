using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Continue(Token keyword) : Statement
{
    public Token Keyword { get; } = keyword;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitContinueStatement(this);
    public override List<Token> GetTokens() => [Keyword];
}