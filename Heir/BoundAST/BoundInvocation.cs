using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundInvocation(BoundExpression callee, List<BoundExpression> arguments) : BoundExpression
{
    public override BaseType Type => Callee.Type is FunctionType functionType
        ? functionType.ReturnType
        : Callee.Type;

    public BoundExpression Callee { get; } = callee;
    public List<BoundExpression> Arguments { get; } = arguments;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundInvocationExpression(this);
    public override List<Token> GetTokens() => Callee.GetTokens();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundInvocation(");
        Callee.Display(indent + 1);
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