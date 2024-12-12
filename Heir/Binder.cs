using Heir.AST;
using Heir.BoundAST;

namespace Heir
{
    enum Context
    {
        Global,
        Parameters
    }

    public sealed class Binder(DiagnosticBag diagnostics, SyntaxTree syntaxTree) : Statement.Visitor<BoundStatement>, Expression.Visitor<BoundExpression>
    {
        public DiagnosticBag Diagnostics { get; } = diagnostics;

        private readonly SyntaxTree _syntaxTree = syntaxTree;
        private readonly Dictionary<SyntaxNode, BoundSyntaxNode> _boundNodes = new();
        private Context _context = Context.Global;

        public BoundStatement Bind() => Bind(_syntaxTree);

        public BoundStatement GetBoundNode(Statement statement) => (BoundStatement)_boundNodes[statement];
        public BoundExpression GetBoundNode(Expression expression) => (BoundExpression)_boundNodes[expression];
        public BoundSyntaxNode GetBoundNode(SyntaxNode expression) => _boundNodes[expression];

        public BoundStatement VisitSyntaxTree(SyntaxTree syntaxTree) => new BoundSyntaxTree(BindStatements(syntaxTree.Statements));

        public BoundStatement VisitBlock(Block block)
        {
            throw new NotImplementedException();
        }

        public BoundExpression VisitAssignmentOpExpression(AssignmentOp assignmentOp)
        {
            throw new NotImplementedException();
        }

        public BoundExpression VisitBinaryOpExpression(BinaryOp binaryOp)
        {
            var left = Bind(binaryOp.Left);
            var right = Bind(binaryOp.Right);
            var boundOperator = BoundBinaryOperator.Bind(binaryOp.Operator, left.Type, right.Type);
            if (boundOperator == null)
            {
                Diagnostics.Error("H010", $"Cannot apply operator \"{binaryOp.Operator.Text}\" to operands of type \"{left.Type.ToString()}\" and \"{right.Type.ToString()}\"", binaryOp.Operator);
                return new BoundNoOp();
            }

            return new BoundBinaryOp(left, boundOperator, right);
        }

        public BoundExpression VisitIdentifierNameExpression(IdentifierName identifierName)
        {
            throw new NotImplementedException();
        }

        public BoundExpression VisitLiteralExpression(Literal literal) => new BoundLiteral(literal.Token);

        public BoundExpression VisitNoOp(NoOp noOp)
        {
            throw new NotImplementedException();
        }

        public BoundExpression VisitParenthesizedExpression(Parenthesized parenthesized)
        {
            throw new NotImplementedException();
        }

        public BoundExpression VisitUnaryOpExpression(UnaryOp unaryOp)
        {
            throw new NotImplementedException();
        }

        //private List<BoundStatement> BindStatements(List<Statement> statements) => statements.ConvertAll(Bind);
        private List<BoundSyntaxNode> BindStatements(List<SyntaxNode> statements) => statements.ConvertAll(Bind); // temp

        private BoundSyntaxNode Bind(SyntaxNode node)
        {
            if (node is Expression expression)
                return Bind(expression);
            else if (node is Statement statement)
                return Bind(statement);

            return null!; // poop
        }

        private BoundStatement Bind(Statement statement)
        {
            var boundStatement = statement.Accept(this);
            _boundNodes.Add(statement, boundStatement);
            return boundStatement;
        }

        private BoundExpression Bind(Expression expression)
        {
            var boundExpression = expression.Accept(this);
            _boundNodes.Add(expression, boundExpression);
            return boundExpression;
        }
    }
}
