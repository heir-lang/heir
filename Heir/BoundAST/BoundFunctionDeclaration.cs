using Heir.Binding;
using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundFunctionDeclaration(
    Token keyword,
    VariableSymbol<FunctionType> symbol,
    List<BoundParameter> parameters,
    BoundBlock body) : BoundStatement
{
    public override FunctionType Type => Symbol.Type;
    
    public Token Keyword { get; } = keyword;
    public VariableSymbol<FunctionType> Symbol { get; } = symbol;
    public List<BoundParameter> Parameters { get; } = parameters;
    public BoundBlock Body { get; } = body;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundFunctionDeclaration(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        Symbol.Name,
        ..Parameters.SelectMany(parameter => parameter.GetTokens()).ToList(),
        ..Body.GetTokens()
    ];
}