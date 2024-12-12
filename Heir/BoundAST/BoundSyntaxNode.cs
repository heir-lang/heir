using Heir.AST;
using Heir.Types;

namespace Heir.BoundAST
{
    public abstract class BoundStatement : BoundSyntaxNode
    {
        public abstract BaseType? Type { get; }

        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
        {
            public R VisitBoundSyntaxTree(BoundSyntaxTree boundBlock);
            public R VisitBoundBlockStatement(BoundBlock boundBlock);
        }
    }

    public abstract class BoundExpression : BoundSyntaxNode
    {
        public abstract BaseType Type { get; }

        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
        {
            //public R VisitBoundIdentifierNameExpression(BoundIdentifierName identifierName);
            //public R VisitBoundAssignmentOpExpression(BoundAssignmentOp assignmentOp);
            //public R VisitBoundUnaryOpExpression(BoundUnaryOp unaryOp);
            public R VisitBoundBinaryOpExpression(BoundBinaryOp binaryOp);
            //public R VisitBoundParenthesizedExpression(BoundParenthesized parenthesized);
            public R VisitBoundLiteralExpression(BoundLiteral literal);
            public R VisitBoundNoOp(BoundNoOp noOp);
        }
    }

    public abstract class BoundSyntaxNode : SyntaxNode
    {
    }
}
