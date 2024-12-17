using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundParenthesized(BoundExpression expression) : BoundExpression
    {
        public override BaseType Type => Expression.Type;

        public BoundExpression Expression { get; } = expression;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundParenthesizedExpression(this);
        public override List<Token> GetTokens() => [];

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundParenthesized(");
            Expression.Display(indent + 1);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
