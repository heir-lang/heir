using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundContinue(Token keyword) : BoundStatement
{
    public override BaseType? Type => null;

    public Token Keyword { get; } = keyword;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundContinueStatement(this);
    public override List<Token> GetTokens() => [Keyword];
}