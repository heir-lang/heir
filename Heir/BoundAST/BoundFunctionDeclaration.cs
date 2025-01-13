using Heir.Binding;
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

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundFunctionDeclaration(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Symbol -> {Symbol.ToString()},");
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Parameters -> [");
        var i = 0;
        foreach (var parameter in Parameters)
        {
            parameter.Display(indent + 2);
            if (i++ < Parameters.Count - 1)
                Console.WriteLine(',');
        }
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}]");
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Body ->");
        Body.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}