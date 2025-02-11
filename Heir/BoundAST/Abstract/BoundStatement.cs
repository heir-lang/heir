using Heir.Types;

namespace Heir.BoundAST.Abstract;

public abstract class BoundStatement : BoundSyntaxNode
{
    public abstract BaseType? Type { get; }

    public abstract R Accept<R>(IVisitor<R> visitor);

    public interface IVisitor<out R>
    {
        public R VisitBoundSyntaxTree(BoundSyntaxTree tree);
        public R VisitBoundBlock(BoundBlock boundBlock);
        public R VisitBoundVariableDeclaration(BoundVariableDeclaration variableDeclaration);
        public R VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement);
        public R VisitBoundNoOp(BoundNoOpStatement noOp);
        public R VisitBoundReturnStatement(BoundReturn @return);
        public R VisitBoundFunctionDeclaration(BoundFunctionDeclaration declaration);
        public R VisitBoundIfStatement(BoundIf @if);
        public R VisitBoundWhileStatement(BoundWhile @while);
    }
}