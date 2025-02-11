namespace Heir.AST.Abstract;

public abstract class Statement : SyntaxNode
{
    public abstract R Accept<R>(IVisitor<R> visitor);

    public interface IVisitor<out R>
    {
        public R VisitSyntaxTree(SyntaxTree tree);
        public R VisitVariableDeclaration(VariableDeclaration variableDeclaration);
        public R VisitBlock(Block block);
        public R VisitExpressionStatement(ExpressionStatement expressionStatement);
        public R VisitNoOp(NoOpStatement noOp);
        public R VisitReturnStatement(Return @return);
        public R VisitFunctionDeclaration(FunctionDeclaration functionDeclaration);
        public R VisitIfStatement(If @if);
        public R VisitWhileStatement(While @while);
        public R VisitInterfaceField(InterfaceField interfaceField);
        public R VisitInterfaceDeclaration(InterfaceDeclaration interfaceDeclaration);
    }
}