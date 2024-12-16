using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundUnaryOp(BoundUnaryOperator op, BoundExpression operand) : BoundExpression
    {
        public override BaseType Type => Operator.ResultType;
        public BoundUnaryOperator Operator { get; } = op;
        public BoundExpression Operand { get; } = operand;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundUnaryOpExpression(this);
        public override List<Token> GetTokens() => Operand.GetTokens();

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundUnaryOp(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {SyntaxFacts.OperatorMap.GetKey(Operator.SyntaxKind)},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operand ->");
            Operand.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
