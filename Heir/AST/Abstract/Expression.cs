namespace Heir.AST.Abstract;

public abstract class Expression : SyntaxNode
{
    public abstract R Accept<R>(Visitor<R> visitor);

    public interface Visitor<out R>
    {
        public R VisitIdentifierNameExpression(IdentifierName identifierName);
        public R VisitAssignmentOpExpression(AssignmentOp assignmentOp);
        public R VisitUnaryOpExpression(UnaryOp unaryOp);
        public R VisitBinaryOpExpression(BinaryOp binaryOp);
        public R VisitParenthesizedExpression(Parenthesized parenthesized);
        public R VisitLiteralExpression(Literal literal);
        public R VisitObjectLiteralExpression(ObjectLiteral objectLiteral);
        public R VisitNoOp(NoOp noOp);
        public R VisitNoOp(NoOpType noOp);
        public R VisitSingularTypeRef(SingularType singularType);
        public R VisitParenthesizedTypeRef(ParenthesizedType parenthesizedType);
        public R VisitUnionTypeRef(UnionType unionType);
        public R VisitIntersectionTypeRef(IntersectionType intersectionType);
        public R VisitParameter(Parameter parameter);
    }
}