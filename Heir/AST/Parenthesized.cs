using Heir.Syntax;

namespace Heir.AST
{
    public class Parenthesized(Expression expression) : Expression
    {
        public Expression Expression { get; } = expression;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitParenthesizedExpression(this);
        public override List<Token> GetTokens() => Expression.GetTokens();

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Parenthesized(");
            Expression.Display(indent + 1);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
