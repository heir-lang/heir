using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Break(Token keyword) : Statement
{
    public Token Keyword { get; } = keyword;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBreakStatement(this);
    public override List<Token> GetTokens() => [Keyword];
}