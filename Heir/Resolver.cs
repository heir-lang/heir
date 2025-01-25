using Heir.AST;
using Heir.AST.Abstract;
using Heir.Runtime.Intrinsics;
using Heir.Syntax;
using Void = Heir.AST.Abstract.Void;

namespace Heir;

public enum ScopeContext
{
    Global,
    Block,
    Class
}

public sealed class Resolver(DiagnosticBag diagnostics, SyntaxTree syntaxTree) : INodeVisitor
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

    public Void VisitSyntaxTree(SyntaxTree tree)
    {
        BeginScope();
        Intrinsics.RegisterResolverGlobals(this);
        ResolveStatements(tree.Statements);
        EndScope();
        
        return default;
    }

    public Void VisitInterfaceField(InterfaceField interfaceField)
    {
        Resolve(interfaceField.Type);
        return default;
    }

    public Void VisitInterfaceDeclaration(InterfaceDeclaration interfaceDeclaration)
    {
        Declare(interfaceDeclaration.Identifier);
        Define(interfaceDeclaration.Identifier);
        foreach (var field in interfaceDeclaration.Fields)
            Resolve(field);

        return default;
    }

    public Void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        Declare(variableDeclaration.Name.Token);
        if (variableDeclaration.Type != null)
            Resolve(variableDeclaration.Type);
        if (variableDeclaration.Initializer != null)
            Resolve(variableDeclaration.Initializer);

        Define(variableDeclaration.Name.Token);
        return default;
    }

    public Void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        Declare(functionDeclaration.Name.Token);
        Define(functionDeclaration.Name.Token);
        return ResolveFunction(functionDeclaration);
    }

    public Void VisitBlock(Block block)
    {
        var enclosingContext = _scopeContext;
        _scopeContext = ScopeContext.Block;
        BeginScope();
        ResolveStatements(block.Statements);
        EndScope();
        _scopeContext = enclosingContext;

        return default;
    }

    public Void VisitReturnStatement(Return @return)
    {
        if (!_withinFunction)
            Diagnostics.Error(DiagnosticCode.H015, "Invalid return statement: Can only use 'return' within a function body", @return.Keyword);
        
        Resolve(@return.Expression);
        return default;
    }

    public Void VisitIfStatement(If @if)
    {
        Resolve(@if.Condition);
        Resolve(@if.Body);
        if (@if.ElseBranch == null)
            return default;
        
        Resolve(@if.ElseBranch);
        return default;
    }

    public Void VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        Resolve(expressionStatement.Expression);
        return default;
    }
    
    public Void VisitMemberAccessExpression(MemberAccess memberAccess)
    {
        Resolve(memberAccess.Expression);
        return default;
    }

    public Void VisitElementAccessExpression(ElementAccess elementAccess)
    {
        Resolve(elementAccess.Expression);
        Resolve(elementAccess.IndexExpression);
        return default;
    }

    public Void VisitInvocationExpression(Invocation invocation)
    {
        Resolve(invocation.Callee);
        foreach (var argument in invocation.Arguments)
            Resolve(argument);

        return default;
    }

    public Void VisitParameter(Parameter parameter)
    {
        Declare(parameter.Name.Token);
        Define(parameter.Name.Token);
        if (parameter.Type != null)
            Resolve(parameter.Type);
        
        return default;
    }

    public Void VisitAssignmentOpExpression(AssignmentOp assignmentOp) => VisitBinaryOpExpression(assignmentOp);
    public Void VisitBinaryOpExpression(BinaryOp binaryOp)
    {
        Resolve(binaryOp.Left);
        Resolve(binaryOp.Right);
        return default;
    }

    public Void VisitIdentifierNameExpression(IdentifierName identifierName)
    {
        var scope = _scopes.LastOrDefault();
        var name = identifierName.Token.Text;
        if (scope != null && scope.TryGetValue(name, out var value) && value == false)
        {
            Diagnostics.Error(DiagnosticCode.H010, $"Cannot read variable '{name}' in it's own initializer", identifierName.Token);
            return default;
        }
        
        if (!IsDefined(identifierName.Token))
        {
            Diagnostics.Error(DiagnosticCode.H011, $"Cannot find name '{name}'", identifierName.Token);
            return default;
        }

        return default;
    }

    public Void VisitLiteralExpression(Literal literal) => default;
    public Void VisitObjectLiteralExpression(ObjectLiteral objectLiteral)
    {
        foreach (var pair in objectLiteral.Properties)
        {
            Resolve(pair.Key);
            Resolve(pair.Value);
        }
        
        return default;
    }

    public Void VisitNoOp(NoOp noOp) => default;
    public Void VisitNoOp(NoOpStatement noOp) => default;
    public Void VisitNoOp(NoOpType noOp) => default;

    public Void VisitSingularTypeRef(SingularType singularType) => default;
    public Void VisitParenthesizedTypeRef(ParenthesizedType parenthesizedType) => default;
    public Void VisitUnionTypeRef(UnionType unionType) => default;
    public Void VisitIntersectionTypeRef(IntersectionType intersectionType) => default;
    public Void VisitFunctionTypeRef(FunctionType functionType) => default;

    public Void VisitParenthesizedExpression(Parenthesized parenthesized)
    {
        Resolve(parenthesized.Expression);
        return default;
    }

    public Void VisitUnaryOpExpression(UnaryOp unaryOp)
    {
        Resolve(unaryOp.Operand);
        return default;
    }

    private Void ResolveFunction(FunctionDeclaration functionDeclaration)
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
        return default;
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