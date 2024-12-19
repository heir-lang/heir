using Heir.AST;
using Heir.Syntax;

namespace Heir
{
    public enum ScopeContext
    {
        Global,
        Block,
        Class
    }

    public sealed class Resolver(DiagnosticBag diagnostics, SyntaxTree syntaxTree) : Expression.Visitor<object?>, Statement.Visitor<object?>
    {
        public DiagnosticBag Diagnostics { get; } = diagnostics;

        private readonly SyntaxTree _syntaxTree = syntaxTree;
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
            var name = identifierName.Token.Text;
            if (scope != null && scope.ContainsKey(name) && scope[name] == false)
            {
                Diagnostics.Error(DiagnosticCode.H010, $"Cannot read variable '{name}' in it's own initializer", identifierName.Token);
                return null;
            }
            if (!IsDefined(identifierName.Token))
            {
                Diagnostics.Error(DiagnosticCode.H011, $"'{name}' is not defined in this scope", identifierName.Token);
                return null;
            }

            return null;
        }

        public object? VisitLiteralExpression(Literal literal) => null;
        public object? VisitObjectLiteralExpression(ObjectLiteral objectLiteral)
        {
            foreach (var pair in objectLiteral.Properties)
            {
                Resolve(pair.Key);
                Resolve(pair.Value);
            }
            return null;
        }

        public object? VisitNoOp(NoOp noOp) => null;
        public object? VisitNoOp(NoOpType noOp) => null;

        public object? VisitSingularTypeRef(SingularType singularType)
        {
            // TODO: scope resolving just like identifiers
            return null;
        }

        public object? VisitUnionTypeRef(UnionType unionType)
        {
            foreach (var type in unionType.Types)
                Resolve(type);

            return null;
        }

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
            if (scope != null)
                scope[identifier.Text] = true;
        }

        private void Declare(Token identifier)
        {
            if (_scopes.Count == 0) return;

            var scope = _scopes.LastOrDefault();
            if (scope?.ContainsKey(identifier.Text) ?? false)
            {
                Diagnostics.Error(DiagnosticCode.H009, $"Variable '{identifier.Text}' is already declared is this scope", identifier);
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
