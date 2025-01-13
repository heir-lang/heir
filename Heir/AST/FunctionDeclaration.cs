using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class FunctionDeclaration(
    Token keyword,
    IdentifierName name,
    List<Parameter> parameters,
    Block body,
    TypeRef? returnType) : Statement
{
    public Token Keyword { get; } = keyword;
    public IdentifierName Name { get; } = name;
    public List<Parameter> Parameters { get; } = parameters;
    public Block Body { get; } = body;
    public TypeRef? ReturnType { get; } = returnType;
    
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitFunctionDeclaration(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        Name.Token,
        ..Parameters.SelectMany(parameter => parameter.GetTokens()).ToList(),
        ..Body.GetTokens(),
        ..ReturnType?.GetTokens() ?? []
    ];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}FunctionDeclaration(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Name ->");
        Name.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Parameters -> [");
        var i = 0;
        foreach (var parameter in Parameters)
        {
            parameter.Display(indent + 2);
            if (i++ < parameters.Count - 1)
                Console.WriteLine(',');
        }
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}]");
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Body ->");
        Body.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}ReturnType -> {(ReturnType == null ? "(inferred)" : "\n")}");
        ReturnType?.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}