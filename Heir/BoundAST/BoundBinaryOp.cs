using Heir.BoundAST.Abstract;
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
    }
}
