using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class Parameter(IdentifierName name, TypeRef? type, Literal? initializer) : Expression
{
    public IdentifierName Name { get; } = name;
    public TypeRef? Type { get; } = type;
    public Literal? Initializer { get; } = initializer;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitParameter(this);
    public override List<Token> GetTokens() => Name.GetTokens().Concat(Initializer?.GetTokens() ?? []).ToList();
}