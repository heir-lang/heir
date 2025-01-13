using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundInvocation(BoundExpression expression, List<BoundExpression> arguments) : BoundExpression
{
    public override BaseType Type => Expression.Type is FunctionType functionType
        ? functionType.ReturnType
        : Expression.Type;

    public BoundExpression Expression { get; } = expression;
    public List<BoundExpression> Arguments { get; } = arguments;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundInvocationExpression(this);
    public override List<Token> GetTokens() => Expression.GetTokens();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundInvocation(");
        Expression.Display(indent + 1);
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