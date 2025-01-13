using Heir.Binding;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundIdentifierName(Token token, VariableSymbol symbol) : BoundName
{
    public override BaseType Type => Symbol.Type;
    public Token Token { get; } = token;
    public VariableSymbol Symbol { get; } = symbol;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundIdentifierNameExpression(this);
    public override List<Token> GetTokens() => [Token];

    public override void Display(int indent = 0) =>
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundIdentifierName({Token.Text})");
}