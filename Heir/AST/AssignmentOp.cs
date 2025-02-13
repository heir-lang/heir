using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class AssignmentOp(AssignmentTarget target, Token op, Expression right) : BinaryOp(target, op, right)
{
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitAssignmentOpExpression(this);

    public AssignmentOp WithLeft(AssignmentTarget left) => new(left, Operator, Right);
    public new AssignmentOp WithOperator(Token op) => new(target, op, Right);
    public new AssignmentOp WithRight(Expression right) => new(target, Operator, right);
}