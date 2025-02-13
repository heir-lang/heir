using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.AST;

public sealed class EnumMember(IdentifierName name, Literal value) : Statement
{
    public IdentifierName Name { get; } = name;
    public Literal Value { get; } = value;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitEnumMember(this);
    public override List<Token> GetTokens() => [..Name.GetTokens(), ..Value?.GetTokens() ?? []];
}