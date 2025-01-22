using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class SingularType(Token token) : TypeRef
{
    public Token Token { get; } = token;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitSingularTypeRef(this);
    public override List<Token> GetTokens() => [Token];
}