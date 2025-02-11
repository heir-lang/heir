using Heir.Types;

namespace Heir.BoundAST.Abstract;

public abstract class BoundExpression : BoundSyntaxNode
{
    public abstract BaseType Type { get; }

    public abstract R Accept<R>(IVisitor<R> visitor);

    public interface IVisitor<out R>
    {
        public R VisitBoundIdentifierNameExpression(BoundIdentifierName identifierName);
        public R VisitBoundAssignmentOpExpression(BoundAssignmentOp assignmentOp);
        public R VisitBoundUnaryOpExpression(BoundUnaryOp unaryOp);
        public R VisitBoundPostfixOpExpression(BoundPostfixOp postfixOp);
        public R VisitBoundBinaryOpExpression(BoundBinaryOp binaryOp);
        public R VisitBoundParenthesizedExpression(BoundParenthesized parenthesized);
        public R VisitBoundLiteralExpression(BoundLiteral literal);
        public R VisitBoundObjectLiteralExpression(BoundObjectLiteral objectLiteral);
        public R VisitBoundNoOp(BoundNoOp noOp);
        public R VisitBoundParameter(BoundParameter boundParameter);
        public R VisitBoundInvocationExpression(BoundInvocation invocation);
        public R VisitBoundElementAccessExpression(BoundElementAccess elementAccess);
        public R VisitBoundMemberAccessExpression(BoundMemberAccess memberAccess);
    }
}