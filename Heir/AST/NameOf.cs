using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class NameOf(Token keyword, IdentifierName name) : Expression
{
    public Token Keyword { get; } = keyword;
    public IdentifierName Name { get; } = name;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitNameOfExpression(this);
    public override List<Token> GetTokens() => [Keyword, ..Name.GetTokens()];
}