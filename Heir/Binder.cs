using Heir.AST;
using Heir.BoundAST;
using Heir.Syntax;

namespace Heir
{
    enum Context
    {
        Global,
        Parameters
    }

    public sealed class Binder(SyntaxTree syntaxTree) : Statement.Visitor<BoundStatement>, Expression.Visitor<BoundExpression>
    {
        private readonly SyntaxTree _syntaxTree = syntaxTree;
        private readonly DiagnosticBag _diagnostics = syntaxTree.Diagnostics;
        private readonly Dictionary<SyntaxNode, BoundSyntaxNode> _boundNodes = [];
        private readonly Stack<Stack<VariableSymbol>> _variableScopes = [];
        private Context _context = Context.Global;

        public BoundSyntaxTree Bind() => (BoundSyntaxTree)Bind(_syntaxTree);

        public BoundStatement GetBoundNode(Statement statement) => (BoundStatement)_boundNodes[statement];
        public BoundExpression GetBoundNode(Expression expression) => (BoundExpression)_boundNodes[expression];
        public BoundSyntaxNode GetBoundNode(SyntaxNode expression) => _boundNodes[expression];

        public BoundStatement VisitSyntaxTree(SyntaxTree syntaxTree) =>
            new BoundSyntaxTree(BindStatements(syntaxTree.Statements), _diagnostics);

        public BoundStatement VisitBlock(Block block) => new BoundBlock(BindStatements(block.Statements));

        public BoundStatement VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            var initializer = variableDeclaration.Initializer != null ? Bind(variableDeclaration.Initializer) : null;
            var name = (BoundIdentifierName)Bind(variableDeclaration.Name);
            return new BoundVariableDeclaration(name, initializer, variableDeclaration.IsMutable);
        }

        public BoundStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            var expression = Bind(expressionStatement.Expression);
            return new BoundExpressionStatement(expression);
        }

        public BoundExpression VisitAssignmentOpExpression(AssignmentOp assignmentOp)
        {
            var binary = VisitBinaryOpExpression(assignmentOp) as BoundBinaryOp;
            if (binary == null)
                return new BoundNoOp();

            return new BoundAssignmentOp(binary.Left, binary.Operator, binary.Right);
        }

        public BoundExpression VisitBinaryOpExpression(BinaryOp binaryOp)
        {
            var left = Bind(binaryOp.Left);
            var right = Bind(binaryOp.Right);
            var boundOperator = BoundBinaryOperator.Bind(binaryOp.Operator, left.Type, right.Type);
            if (boundOperator == null)
            {
                _diagnostics.Error("H010", $"Cannot apply operator \"{binaryOp.Operator.Text}\" to operands of type \"{left.Type.ToString()}\" and \"{right.Type.ToString()}\"", binaryOp.Operator);
                return new BoundNoOp();
            }

            return new BoundBinaryOp(left, boundOperator, right);
        }

        public BoundExpression VisitUnaryOpExpression(UnaryOp unaryOp)
        {
            var operand = Bind(unaryOp.Operand);
            var boundOperator = BoundUnaryOperator.Bind(unaryOp.Operator, operand.Type);
            if (boundOperator == null)
            {
                _diagnostics.Error("H010", $"Cannot apply operator \"{unaryOp.Operator.Text}\" to operand of type \"{operand.Type.ToString()}\"", unaryOp.Operator);
                return new BoundNoOp();
            }

            return new BoundUnaryOp(boundOperator, operand);
        }

        public BoundExpression VisitIdentifierNameExpression(IdentifierName identifierName)
        {
            var variableSymbol = FindSymbol(identifierName.Token);
            if (variableSymbol == null)
                return new BoundNoOp();

            return new BoundIdentifierName(identifierName.Token, variableSymbol.Type);
        }

        public BoundExpression VisitLiteralExpression(Literal literal) => new BoundLiteral(literal.Token);
        public BoundStatement VisitNoOp(NoOpStatement noOp) => new BoundNoOpStatement();
        public BoundExpression VisitNoOp(NoOp noOp) => new BoundNoOp();

        public BoundExpression VisitParenthesizedExpression(Parenthesized parenthesized)
        {
            var expression = Bind(parenthesized.Expression);
            return new BoundParenthesized(expression);
        }

        private void BeginScope() => _variableScopes.Push([]);
        private Stack<VariableSymbol> EndScope() => _variableScopes.Pop();

        private VariableSymbol? FindSymbol(Token name)
        {
            var symbol = _variableScopes.SelectMany(v => v).FirstOrDefault(symbol => symbol.Name.Text == name.Text);
            if (symbol != null)
                return symbol;

            _diagnostics.Error("H001E", $"Failed to find variable symbol for '{name.Text}'", name);
            return null;
        }

        private List<BoundStatement> BindStatements(List<Statement> statements) => statements.ConvertAll(Bind);

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
