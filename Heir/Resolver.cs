using Heir.AST;
using Heir.AST.Abstract;
using Heir.Runtime.Intrinsics;
using Heir.Syntax;

namespace Heir;

public enum ScopeContext
{
    Global,
    Block,
    Class
}

public sealed class Resolver(DiagnosticBag diagnostics, SyntaxTree syntaxTree) : Expression.Visitor<object?>, Statement.Visitor<object?>
{
    public DiagnosticBag Diagnostics { get; } = diagnostics;

    private readonly Stack<Dictionary<string, bool>> _scopes = [];
    private ScopeContext _scopeContext = ScopeContext.Global;
    private bool _withinFunction;

    public void Resolve() => Resolve(syntaxTree);
    
    public void Define(Token identifier)
    {
        if (_scopes.Count == 0) return;
        if (!_scopes.TryPeek(out var scope)) return;

        scope[identifier.Text] = true;
    }

    public void Declare(Token identifier)
    {
        if (_scopes.Count == 0) return;
        if (!_scopes.TryPeek(out var scope)) return;
        if (scope.TryAdd(identifier.Text, false)) return;
            
        Diagnostics.Error(DiagnosticCode.H009, $"Variable '{identifier.Text}' is already declared in this scope", identifier);
    }

    public object? VisitSyntaxTree(SyntaxTree tree)
    {
        BeginScope();
        Intrinsics.RegisterResolverGlobals(this);
        ResolveStatements(tree.Statements);
        EndScope();
        
        return null;
    }

    public object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        Declare(variableDeclaration.Name.Token);
        if (variableDeclaration.Type != null)
            Resolve(variableDeclaration.Type);
        if (variableDeclaration.Initializer != null)
            Resolve(variableDeclaration.Initializer);

        Define(variableDeclaration.Name.Token);
        return null;
    }

    public object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        Declare(functionDeclaration.Name.Token);
        Define(functionDeclaration.Name.Token);
        return ResolveFunction(functionDeclaration);
    }

    public object? VisitBlock(Block block)
    {
        var enclosingContext = _scopeContext;
        _scopeContext = ScopeContext.Block;
        BeginScope();
        ResolveStatements(block.Statements);
        EndScope();
        _scopeContext = enclosingContext;

        return null;
    }

    public object? VisitReturnStatement(Return @return)
    {
        if (!_withinFunction)
            Diagnostics.Error(DiagnosticCode.H015, "Invalid return statement: Can only use 'return' within a function body", @return.Keyword);
            
        Resolve(@return.Expression);
        return null;
    }

    public object? VisitIfStatement(If @if)
    {
        Resolve(@if.Condition);
        Resolve(@if.Body);
        if (@if.ElseBranch == null)
            return null;
        
        Resolve(@if.ElseBranch);
        return null;
    }

    public object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        Resolve(expressionStatement.Expression);
        return null;
    }

    public object? VisitElementAccessExpression(ElementAccess elementAccess)
    {
        Resolve(elementAccess.Expression);
        Resolve(elementAccess.IndexExpression);
        return null;
    }

    public object? VisitInvocationExpression(Invocation invocation)
    {
        Resolve(invocation.Callee);
        foreach (var argument in invocation.Arguments)
            Resolve(argument);

        return null;
    }

    public object? VisitParameter(Parameter parameter)
    {
        Declare(parameter.Name.Token);
        Define(parameter.Name.Token);
        if (parameter.Type != null)
            Resolve(parameter.Type);
        
        return null;
    }

    public object? VisitAssignmentOpExpression(AssignmentOp assignmentOp) => VisitBinaryOpExpression(assignmentOp);
    public object? VisitBinaryOpExpression(BinaryOp binaryOp)
    {
        Resolve(binaryOp.Left);
        Resolve(binaryOp.Right);
        return null;
    }

    public object? VisitIdentifierNameExpression(IdentifierName identifierName)
    {
        var scope = _scopes.LastOrDefault();
        var name = identifierName.Token.Text;
        if (scope != null && scope.TryGetValue(name, out var value) && value == false)
        {
            Diagnostics.Error(DiagnosticCode.H010, $"Cannot read variable '{name}' in it's own initializer", identifierName.Token);
            return null;
        }
        if (!IsDefined(identifierName.Token))
        {
            Diagnostics.Error(DiagnosticCode.H011, $"Cannot find name '{name}'", identifierName.Token);
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
        var scope = _scopes.LastOrDefault();
        var name = singularType.Token.Text;
        if (scope != null && scope.TryGetValue(name, out var value) && value == false)
        {
            Diagnostics.Error(DiagnosticCode.H010, $"Cannot read type '{name}' in it's own declaration", singularType.Token);
            return null;
        }
        if (!IsDefined(singularType.Token) && !SyntaxFacts.TypeSyntaxes.Contains(singularType.Token.Kind))
        {
            Diagnostics.Error(DiagnosticCode.H011, $"Cannot find name '{name}'", singularType.Token);
            return null;
        }
        
        return null;
    }

    public object? VisitParenthesizedTypeRef(ParenthesizedType parenthesizedType)
    {
        Resolve(parenthesizedType.Type);
        return null;
    }

    public object? VisitUnionTypeRef(UnionType unionType)
    {
        foreach (var type in unionType.Types)
            Resolve(type);

        return null;
    }
        
    public object? VisitIntersectionTypeRef(IntersectionType intersectionType)
    {
        foreach (var type in intersectionType.Types)
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

    private object? ResolveFunction(FunctionDeclaration functionDeclaration)
    {
        var enclosingWithin = _withinFunction;
        _withinFunction = true;
        BeginScope();
        if (functionDeclaration.ReturnType != null)
            Resolve(functionDeclaration.ReturnType);

        foreach (var parameter in functionDeclaration.Parameters)
            Resolve(parameter);

        ResolveStatements(functionDeclaration.Body.Statements);
        EndScope();

        _withinFunction = enclosingWithin;
        return null;
    }

    private bool IsDefined(Token identifier)
    {
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            var scope = _scopes.ElementAtOrDefault(i);
            if (scope != null && scope.TryGetValue(identifier.Text, out var value))
                return value;
        }

        return false;
    }

    private void BeginScope() => _scopes.Push([]);
    private void EndScope() => _scopes.Pop();

    private void ResolveStatements(List<Statement> statements) => statements.ForEach(Resolve);
    public void Resolve(Expression expression) => expression.Accept(this);
    public void Resolve(Statement statement) => statement.Accept(this);
    private void Resolve(SyntaxNode node)
    {
        switch (node)
        {
            case Expression expression:
                Resolve(expression);
                break;
            case Statement statement:
                Resolve(statement);
                break;
        }
    }
}