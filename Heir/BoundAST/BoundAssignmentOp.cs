using Heir.Syntax;

namespace Heir.BoundAST
{
    public class BoundAssignmentOp(BoundExpression left, BoundBinaryOperator op, BoundExpression right) : BoundBinaryOp(left, op, right)
    {
        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundAssignmentOpExpression(this);

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundAssignmentOp(");
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
