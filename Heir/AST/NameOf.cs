using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class NameOf(IdentifierName name) : Expression
{
    public IdentifierName Name { get; } = name;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitNameOfExpression(this);
    public override List<Token> GetTokens() => Name.GetTokens();
}