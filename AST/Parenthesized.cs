namespace Heir.AST
{
    public class Parenthesized(Expression expression) : Expression
    {
        public Expression Expression { get; } = expression;

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Parenthesized(");
            Expression.Display(indent + 1);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
