using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ExpressionStatement(Expression expression) : Statement
{
    public Expression Expression { get; } = expression;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitExpressionStatement(this);
    public override List<Token> GetTokens() => Expression.GetTokens();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}ExpressionStatement(");
        Expression.Display(indent + 1);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}