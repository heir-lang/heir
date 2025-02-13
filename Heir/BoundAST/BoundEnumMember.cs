using Heir.Syntax;
using Heir.BoundAST.Abstract;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundEnumMember(Token name, BoundLiteral value) : BoundStatement
{
    public override LiteralType Type => Value.Type;
    
    public Token Name { get; } = name;
    public BoundLiteral Value { get; } = value;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundEnumMember(this);
    public override List<Token> GetTokens() => [Name, ..Value.GetTokens()];
}