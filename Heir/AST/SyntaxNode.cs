using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public abstract class Statement : SyntaxNode
    {
        public abstract class Visitor<R>
        {
            public abstract R VisitSyntaxTree(SyntaxTree syntaxTree);
        }
    }

    public abstract class Expression : SyntaxNode
    {
        public abstract class Visitor<R>
        {
            public abstract R VisitIdentifierNameExpression(IdentifierName identifierName);
            public abstract R VisitAssignmentOpExpression(AssignmentOp assignmentOp);
            public abstract R VisitUnaryOpExpression(UnaryOp unaryOp);
            public abstract R VisitBinaryOpExpression(BinaryOp binaryOp);
            public abstract R VisitParenthesizedExpression(Parenthesized parenthesized);
            public abstract R VisitLiteralExpression(Literal literal);
            public abstract R VisitNoOp(NoOp noOp);
        }
    }

    public abstract class SyntaxNode
    {
        public abstract List<Instruction> GenerateBytecode();
        public abstract void Display(int indent = 0);
        public abstract List<Token> GetTokens();

        public Token GetFirstToken() => GetTokens().First();
        public Token GetLastToken() => GetTokens().Last();
        public bool Is<T>() where T : SyntaxNode => this is T;
    }
}
