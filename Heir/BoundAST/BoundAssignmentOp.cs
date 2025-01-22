using Heir.BoundAST.Abstract;

namespace Heir.BoundAST
{
    public class BoundAssignmentOp(BoundExpression left, BoundBinaryOperator op, BoundExpression right) : BoundBinaryOp(left, op, right)
    {
        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundAssignmentOpExpression(this);
    }
}
