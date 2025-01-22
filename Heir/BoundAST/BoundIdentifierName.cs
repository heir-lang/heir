using Heir.Binding;
using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundIdentifierName(VariableSymbol<BaseType> symbol) : BoundName
{
    public override BaseType Type => Symbol.Type;
    public VariableSymbol<BaseType> Symbol { get; } = symbol;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundIdentifierNameExpression(this);
    public override List<Token> GetTokens() => [Symbol.Name];
}