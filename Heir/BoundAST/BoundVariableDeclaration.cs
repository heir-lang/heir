using Heir.Binding;
using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundVariableDeclaration(VariableSymbol<BaseType> symbol, BoundExpression? initializer, bool isMutable) : BoundStatement
{
    public override BaseType? Type => Symbol.Type;

    public VariableSymbol<BaseType> Symbol { get; } = symbol;
    public BoundExpression? Initializer { get; } = initializer;
    public bool IsMutable { get; } = isMutable;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundVariableDeclaration(this);
    public override List<Token> GetTokens() => [Symbol.Name, ..Initializer?.GetTokens() ?? []];
}