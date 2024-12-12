using Heir.Syntax;

namespace Heir.AST
{
    public class AssignmentOp(Expression left, Token op, Expression right) : BoundBinaryOp(left, op, right)
    {
        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}AssignmentOp(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Left ->");
            Left.Display(indent + 2);
            Console.WriteLine(",");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Right ->");
            Right.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
