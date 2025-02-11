using Heir.Binding;
using Heir.BoundAST.Abstract;

namespace Heir.BoundAST
{
    public class BoundAssignmentOp(BoundExpression left, BoundBinaryOperator op, BoundExpression right) : BoundBinaryOp(left, op, right)
    {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundAssignmentOpExpression(this);
    }
}
