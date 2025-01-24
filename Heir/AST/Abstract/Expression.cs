namespace Heir.AST.Abstract;

public abstract class Expression : SyntaxNode
{
    public abstract R Accept<R>(IVisitor<R> visitor);

    public interface IVisitor<out R>
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
        public R VisitFunctionTypeRef(FunctionType functionType);
        public R VisitParameter(Parameter parameter);
        public R VisitInvocationExpression(Invocation invocation);
        public R VisitElementAccessExpression(ElementAccess elementAccess);
        public R VisitMemberAccessExpression(MemberAccess memberAccess);
    }
}