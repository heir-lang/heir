using Heir.Syntax;

namespace Heir.AST
{
    public abstract class Statement : SyntaxNode
    {
        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
        {
            public R VisitSyntaxTree(SyntaxTree syntaxTree);
            public R VisitVariableDeclaration(VariableDeclaration variableDeclaration);
            public R VisitBlock(Block block);
            public R VisitExpressionStatement(ExpressionStatement expressionStatement);
            public R VisitNoOp(NoOpStatement noOp);
        }
    }

    public abstract class TypeRef : Expression;

    public abstract class Expression : SyntaxNode
    {
        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
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
            public R VisitUnionTypeRef(UnionType unionType);
        }
    }

    public abstract class SyntaxNode
    {
        public abstract void Display(int indent = 0);
        public abstract List<Token> GetTokens();

        public Token GetFirstToken() => GetTokens().First();
        public Token GetLastToken() => GetTokens().Last();
        public bool Is<T>() where T : SyntaxNode => this is T;
    }
}
