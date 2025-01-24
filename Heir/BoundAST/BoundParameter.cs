using Heir.Binding;
using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundParameter(VariableSymbol<BaseType> symbol, BoundLiteral? initializer) : BoundExpression
{
    public override BaseType Type => Symbol.Type;

    public VariableSymbol<BaseType> Symbol { get; } = symbol;
    public BoundLiteral? Initializer { get; } = initializer;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundParameter(this);
    public override List<Token> GetTokens() => [Symbol.Name, ..Initializer?.GetTokens() ?? []];
}