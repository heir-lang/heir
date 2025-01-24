using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class AssignmentOp(AssignmentTarget target, Token op, Expression right) : BinaryOp(target, op, right)
{
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitAssignmentOpExpression(this);
}