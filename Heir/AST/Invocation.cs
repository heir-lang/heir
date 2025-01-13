using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Invocation(Expression callee, List<Expression> arguments) : Expression
{
    public Expression Callee { get; } = callee;
    public List<Expression> Arguments { get; } = arguments;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitInvocationExpression(this);
    public override List<Token> GetTokens() => [..Callee.GetTokens(), ..Arguments.SelectMany(argument => argument.GetTokens())];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Invocation(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Callee ->");
        Callee.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Arguments -> [");
        foreach (var argument in Arguments)
        {
            argument.Display(indent + 2);
            Console.WriteLine(',');
        }
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}]");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}