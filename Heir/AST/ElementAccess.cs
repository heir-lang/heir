using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class ElementAccess(Expression expression, Expression indexExpression) : Expression
{
    public Expression Expression { get; } = expression;
    public Expression IndexExpression { get; } = indexExpression;
    
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitElementAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..IndexExpression.GetTokens()];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}ElementAccess(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Expression ->");
        Expression.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}IndexExpression ->");
        IndexExpression.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}