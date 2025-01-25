using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class VariableDeclaration(IdentifierName name, Expression? initializer, TypeRef? type, bool isMutable) : Statement
{
    public IdentifierName Name { get; } = name;
    public Expression? Initializer { get; } = initializer;
    public TypeRef? Type { get; } = type;
    public bool IsMutable { get; } = isMutable;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitVariableDeclaration(this);
    public override List<Token> GetTokens() => Name.GetTokens().Concat(Initializer?.GetTokens() ?? []).ToList();

    public VariableDeclaration WithName(IdentifierName name) => new(name, Initializer, Type, IsMutable);
    public VariableDeclaration WithInitializer(Expression? initializer) => new(Name, initializer, Type, IsMutable);
    public VariableDeclaration WithType(TypeRef? typeRef) => new(Name, Initializer, typeRef, IsMutable);
    public VariableDeclaration WithMutability(bool isMutable) => new(Name, Initializer, Type, isMutable);
}