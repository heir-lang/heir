using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundBinaryOp(BoundExpression left, BoundBinaryOperator op, BoundExpression right) : BoundExpression
    {
        public override BaseType Type => Operator.ResultType;
        public BoundExpression Left { get; } = left;
        public BoundBinaryOperator Operator { get; } = op;
        public BoundExpression Right { get; } = right;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundBinaryOpExpression(this);
        public override List<Token> GetTokens() => Left.GetTokens().Concat(Right.GetTokens()).ToList();

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundBinaryOp(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Left ->");
            Left.Display(indent + 2);
            Console.WriteLine(",");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {SyntaxFacts.OperatorMap.GetKey(Operator.SyntaxKind)},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Right ->");
            Right.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
