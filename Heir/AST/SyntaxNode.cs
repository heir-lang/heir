using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public abstract class Statement : SyntaxNode
    {
        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
        {
            public R VisitSyntaxTree(SyntaxTree syntaxTree);
            public R VisitBlock(Block block);
        }
    }

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
            public R VisitNoOp(NoOp noOp);
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
