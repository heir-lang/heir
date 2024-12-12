using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundBinaryOp(BoundExpression left, Token op, BoundExpression right) : BoundExpression
    {
        public override BaseType Type => throw new NotImplementedException();
        public BoundExpression Left { get; } = left;
        public Token Operator { get; } = op;
        public BoundExpression Right { get; } = right;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundBinaryOpExpression(this);
        public override List<Token> GetTokens() => Left.GetTokens().Append(Operator).Concat(Right.GetTokens()).ToList();

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BinaryOp(");
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
