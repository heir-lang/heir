using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class IdentifierName(Token token) : Name
{
    public Token Token { get; } = token;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitIdentifierNameExpression(this);
    public override List<Token> GetTokens() => [Token];
    
    public override string ToString() => Token.Text;
}