using Heir.AST;
using Heir.Syntax;

namespace Heir
{
    internal enum ScopeContext
    {
        Global,
        Block,
        Class
    }

    internal class Resolver(SyntaxTree syntaxTree) : Expression.Visitor<object?>, Statement.Visitor<object?>
    {
        private readonly SyntaxTree _syntaxTree = syntaxTree;
        private readonly DiagnosticBag _diagnostics = syntaxTree.Diagnostics;
        private readonly List<Dictionary<string, bool>> _scopes = [];
        private ScopeContext _scopeContext = ScopeContext.Global;
        private bool withinFunction = false;

        public void Resolve()
        {
            BeginScope();
            Resolve(_syntaxTree);
        }

        public object? VisitSyntaxTree(SyntaxTree syntaxTree)
        {
            ResolveStatements(syntaxTree.Statements);
            return null;
        }

        public object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            Declare(variableDeclaration.Name.Token);
            if (variableDeclaration.Initializer != null)
                Resolve(variableDeclaration.Initializer);

            Define(variableDeclaration.Name.Token);
            return null;
        }

        public object? VisitBlock(Block block)
        {
            var enclosingContext = _scopeContext;
            _scopeContext = ScopeContext.Block;
            ResolveStatements(block.Statements);
            _scopeContext = enclosingContext;

            return null;
        }

        public object? VisitAssignmentOpExpression(AssignmentOp assignmentOp) => VisitBinaryOpExpression(assignmentOp);
        public object? VisitBinaryOpExpression(BinaryOp binaryOp)
        {
            Resolve(binaryOp.Left);
            Resolve(binaryOp.Right);
            return null;
        }

        public object? VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            Resolve(expressionStatement.Expression);
            return null;
        }

        public object? VisitIdentifierNameExpression(IdentifierName identifierName)
        {
            var scope = _scopes.LastOrDefault();
            if (scope != null && scope[identifierName.Token.Text] == false)
            {
                _diagnostics.Error("H013", $"Cannot read variable '{identifierName.Token.Text}' in it's own initializer", identifierName.Token);
                return null;
            }
            if (!IsDefined(identifierName.Token))
            {
                _diagnostics.Error("H014", $"'{identifierName.Token.Text}' is not defined in this scope", identifierName.Token);
                return null;
            }

            return null;
        }

        public object? VisitLiteralExpression(Literal literal) => null;
        public object? VisitNoOp(NoOp noOp) => null;

        public object? VisitNoOp(NoOpStatement noOp) => null;

        public object? VisitParenthesizedExpression(Parenthesized parenthesized)
        {
            Resolve(parenthesized.Expression);
            return null;
        }

        public object? VisitUnaryOpExpression(UnaryOp unaryOp)
        {
            Resolve(unaryOp.Operand);
            return null;
        }

        private void Define(Token identifier)
        {
            if (_scopes.Count == 0) return;

            var scope = _scopes.LastOrDefault();
            scope?.Add(identifier.Text, true);
        }

        private void Declare(Token identifier)
        {
            if (_scopes.Count == 0) return;

            var scope = _scopes.LastOrDefault();
            if (scope?.ContainsKey(identifier.Text) ?? false)
            {
                _diagnostics.Error("H012", $"Variable '{identifier.Text}' is already declared is this scope", identifier);
                return;
            }

            scope?.Add(identifier.Text, false);
        }

        private bool IsDefined(Token identifier)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes.ElementAtOrDefault(i);
                if (scope != null && scope.ContainsKey(identifier.Text))
                    return scope[identifier.Text];
            }

            return false;
        }

        private void BeginScope()
        {
            _scopes.Add(new());
        }

        private void EndScope()
        {
            _scopes.Remove(_scopes.Last());
        }

        private void ResolveStatements(List<Statement> statements) => statements.ForEach(Resolve);
        private void Resolve(Expression expression) => expression.Accept(this);
        private void Resolve(Statement statement) => statement.Accept(this);
        private void Resolve(SyntaxNode node)
        {
            if (node is Expression expression)
                Resolve(expression);
            else if (node is Statement statement)
                Resolve(statement);
        }
    }
}
