using Heir.Binding;
using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundTypeParameter(TypeSymbol symbol, BaseType? initializer) : BoundExpression
{
    public override BaseType Type => Symbol.Type;

    public TypeSymbol Symbol { get; } = symbol;
    public BaseType? Initializer { get; } = initializer;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundTypeParameter(this);
    public override List<Token> GetTokens() => [Symbol.Name];
}