using Heir.AST;
using Heir.AST.Abstract;
using Heir.Binding;
using Heir.BoundAST;
using Heir.Runtime.Intrinsics;
using Heir.Syntax;
using Heir.Types;

namespace Heir;

using PropertyPair = KeyValuePair<LiteralType, InterfaceMemberSignature>;

public sealed class Binder(DiagnosticBag diagnostics, SyntaxTree syntaxTree) : Statement.Visitor<BoundStatement>, Expression.Visitor<BoundExpression>
{
    public SyntaxTree SyntaxTree { get; } = syntaxTree;

    private readonly Dictionary<SyntaxNode, BoundSyntaxNode> _boundNodes = [];
    private readonly Stack<Stack<VariableSymbol<BaseType>>> _variableScopes = [];
    
    public BoundSyntaxTree Bind()
    {
        BeginScope();
        Intrinsics.RegisterGlobalSymbols(this);
        var tree = (BoundSyntaxTree)Bind(SyntaxTree);
        EndScope();

        return tree;
    }

    public BoundSyntaxTree GetBoundSyntaxTree() => (BoundSyntaxTree)GetBoundNode(SyntaxTree);
    public BoundStatement GetBoundNode(Statement statement) => (BoundStatement)_boundNodes[statement];
    public BoundExpression GetBoundNode(Expression expression) => (BoundExpression)_boundNodes[expression];
    public BoundSyntaxNode GetBoundNode(SyntaxNode node) => _boundNodes[node];
    
    public VariableSymbol<BaseType> DefineSymbol(Token name, BaseType type, bool isMutable) =>
        DefineSymbol<BaseType>(name, type, isMutable);

    public VariableSymbol<TType> DefineSymbol<TType>(Token name, TType type, bool isMutable) where TType : BaseType
    {
        // this is so braindead
        var symbol = new VariableSymbol<TType>(name, type, isMutable);
        if (_variableScopes.TryPeek(out var scope))
            scope.Push(new VariableSymbol<BaseType>(name, type, isMutable));

        return symbol;
    }

    public VariableSymbol<BaseType>? FindSymbol(Token name, bool errorIfNotFound = true)
    {
        var symbol = _variableScopes
            .SelectMany(v => v)
            .FirstOrDefault(symbol => symbol.Name.Text == name.Text);
        
        if (symbol != null)
            return symbol;

        if (errorIfNotFound)
            diagnostics.Error(DiagnosticCode.H005, $"Failed to find variable symbol for '{name.Text}'", name);
        
        return null;
    }

    public BoundStatement VisitSyntaxTree(SyntaxTree tree) =>
        new BoundSyntaxTree(BindStatements(tree.Statements), diagnostics);

    public BoundStatement VisitBlock(Block block) => new BoundBlock(BindStatements(block.Statements));

    public BoundStatement VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var initializer = variableDeclaration.Initializer != null ? Bind(variableDeclaration.Initializer) : null;
        BaseType type;
        if (variableDeclaration.Type != null)
            type = BaseType.FromTypeRef(variableDeclaration.Type);
        else
            type = initializer?.Type ?? IntrinsicTypes.Any;

        var symbol = DefineSymbol(variableDeclaration.Name.Token, type, variableDeclaration.IsMutable);
        return new BoundVariableDeclaration(symbol, initializer, variableDeclaration.IsMutable);
    }

    public BoundStatement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var boundParameters = functionDeclaration.Parameters
            .ConvertAll(Bind)
            .OfType<BoundParameter>()
            .ToList();
        
        var parameterTypePairs = boundParameters.ConvertAll(parameter =>
            new KeyValuePair<string, BaseType>(parameter.Symbol.Name.Text, parameter.Initializer != null
                ? BaseType.Nullable(parameter.Type)
                : parameter.Type));
        
        // TODO: fix this!
        // if heir is left to infer a function return type, then we do not know the
        // return type of a recursive call within the function.
        // in this example the type of x is any:
        // fn abc { let x = abc(); return 123; }
        var parameterTypes = new Dictionary<string, BaseType>(parameterTypePairs);
        var placeholderType = new FunctionType(
            parameterTypes,
            functionDeclaration.ReturnType != null
                ? BaseType.FromTypeRef(functionDeclaration.ReturnType)
                : IntrinsicTypes.Any
        );
        
        var placeholderSymbol = DefineSymbol<BaseType>(functionDeclaration.Name.Token, placeholderType, false);
        var boundBody = (BoundBlock)Bind(functionDeclaration.Body);
        var finalType = new FunctionType(
            parameterTypes,
            functionDeclaration.ReturnType != null
                ? BaseType.FromTypeRef(functionDeclaration.ReturnType)
                : boundBody.Type
        );

        UndefineSymbol(placeholderSymbol);
        var symbol = DefineSymbol(functionDeclaration.Name.Token, finalType, false);
        return new BoundFunctionDeclaration(functionDeclaration.Keyword, symbol, boundParameters, boundBody);
    }

    public BoundStatement VisitReturnStatement(Return @return)
    {
        var expression = Bind(@return.Expression);
        return new BoundReturn(@return.Keyword, expression);
    }

    public BoundStatement VisitIfStatement(If @if)
    {
        var condition = Bind(@if.Condition);
        var body = Bind(@if.Body);
        var elseBranch = @if.ElseBranch != null ? Bind(@if.ElseBranch) : null;
        
        return new BoundIf(@if.Keyword, condition, body, elseBranch);
    }

    public BoundStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = Bind(expressionStatement.Expression);
        return new BoundExpressionStatement(expression);
    }

    public BoundExpression VisitParameter(Parameter parameter)
    {
        var initializer = parameter.Initializer != null ? Bind(parameter.Initializer) : null;
        var type = parameter.Type != null
            ? BaseType.FromTypeRef(parameter.Type)
            : initializer != null ? initializer.Type : IntrinsicTypes.Any;

        var symbol = DefineSymbol(parameter.Name.Token, type, true);
        return new BoundParameter(symbol, initializer);
    }

    public BoundExpression VisitElementAccessExpression(ElementAccess elementAccess)
    {
        var expression = Bind(elementAccess.Expression);
        var indexExpression = Bind(elementAccess.IndexExpression);
        return new BoundElementAccess(expression, indexExpression);
    }

    public BoundExpression VisitInvocationExpression(Invocation invocation)
    {
        var callee = Bind(invocation.Callee);
        var arguments = invocation.Arguments.ConvertAll(Bind);
        return new BoundInvocation(callee, arguments);
    }

    public BoundExpression VisitAssignmentOpExpression(AssignmentOp assignmentOp)
    {
        if (VisitBinaryOpExpression(assignmentOp) is not BoundBinaryOp binary)
            return new BoundNoOp();

        var symbol = FindSymbol(binary.Left.GetFirstToken());
        if (symbol is { IsMutable: false })
            diagnostics.Error(DiagnosticCode.H006C, $"Attempt to assign to immutable variable '{symbol.Name.Text}'", binary.Left, binary.Right);

        return new BoundAssignmentOp(binary.Left, binary.Operator, binary.Right);
    }

    public BoundExpression VisitBinaryOpExpression(BinaryOp binaryOp)
    {
        var left = Bind(binaryOp.Left);
        var right = Bind(binaryOp.Right);
        var boundOperator = BoundBinaryOperator.Bind(binaryOp.Operator, left.Type, right.Type);
        if (boundOperator == null)
        {
            diagnostics.Error(DiagnosticCode.H007, $"Cannot apply operator '{binaryOp.Operator.Text}' to operands of type '{left.Type.ToString()}' and '{right.Type.ToString()}'", binaryOp.Operator);
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
            diagnostics.Error(DiagnosticCode.H007, $"Cannot apply operator '{unaryOp.Operator.Text}' to operand of type '{operand.Type.ToString()}'", unaryOp.Operator);
            return new BoundNoOp();
        }

        return new BoundUnaryOp(boundOperator, operand);
    }

    public BoundExpression VisitIdentifierNameExpression(IdentifierName identifierName)
    {
        var symbol = FindSymbol(identifierName.Token) ??
            new VariableSymbol<BaseType>(identifierName.Token, IntrinsicTypes.Any, false);
        
        return new BoundIdentifierName(symbol);
    }

    public BoundExpression VisitLiteralExpression(Literal literal) => new BoundLiteral(literal.Token);
    public BoundExpression VisitObjectLiteralExpression(ObjectLiteral objectLiteral)
    {
        var propertyPairs = objectLiteral.Properties.ToList();
        var properties = new Dictionary<BaseType, BoundExpression>();
        foreach (var pair in propertyPairs)
        {
            var boundKey = Bind(pair.Key);
            var keyType = boundKey switch
            {
                BoundLiteral { Token.Kind: SyntaxKind.StringLiteral or SyntaxKind.IntLiteral } literal =>
                    new LiteralType(literal.Token.Value),

                _ => boundKey.Type
            };

            properties.Add(keyType, Bind(pair.Value));
        }
        
        var indexSignatures = new Dictionary<PrimitiveType, BaseType>();
        var pairs = properties.ToList();
        var typeProperties = new List<PropertyPair>();
        var index = 0;
        foreach (var pair in pairs)
        {
            if (pair.Key is LiteralType literalType)
            {
                typeProperties.Add(new(literalType, new(pair.Value.Type, isMutable: true)));
                continue;
            }

            if (!pair.Key.IsAssignableTo(IntrinsicTypes.Index))
            {
                var expressionPair = propertyPairs[index++];
                diagnostics.Error(DiagnosticCode.H007, "An index signature type must be 'string' or 'int'", expressionPair.Key.GetFirstToken());
            }

            indexSignatures.Add((PrimitiveType)pair.Key, pair.Value.Type);
        }

        var type = new InterfaceType(new(typeProperties), indexSignatures);
        return new BoundObjectLiteral(objectLiteral.Token, properties, type);
    }

    public BoundStatement VisitNoOp(NoOpStatement noOp) => new BoundNoOpStatement();
    public BoundExpression VisitNoOp(NoOpType noOp) => new BoundNoOp();
    public BoundExpression VisitNoOp(NoOp noOp) => new BoundNoOp();
    public BoundExpression VisitSingularTypeRef(AST.SingularType singularType) => new BoundNoOp();
    public BoundExpression VisitParenthesizedTypeRef(AST.ParenthesizedType singularType) => new BoundNoOp();
    public BoundExpression VisitUnionTypeRef(AST.UnionType unionType) => new BoundNoOp();
    public BoundExpression VisitIntersectionTypeRef(AST.IntersectionType intersectionType) => new BoundNoOp();

    public BoundExpression VisitParenthesizedExpression(Parenthesized parenthesized)
    {
        var expression = Bind(parenthesized.Expression);
        return new BoundParenthesized(expression);
    }

    private void UndefineSymbol(VariableSymbol<BaseType> variableSymbol)
    {
        if (!_variableScopes.TryPop(out var scope)) return;

        var newScope = scope.ToList();
        newScope.Remove(variableSymbol);
        _variableScopes.Push(new Stack<VariableSymbol<BaseType>>(newScope));
    }
    
    private void BeginScope() => _variableScopes.Push([]);
    private void EndScope() => _variableScopes.Pop();

    private List<BoundStatement> BindStatements(List<Statement> statements) => statements.ConvertAll(Bind);

    private BoundSyntaxNode Bind(SyntaxNode node)
    {
        return node switch
        {
            Expression expression => Bind(expression),
            Statement statement => Bind(statement),
            _ => null!
        };
    }

    private BoundStatement Bind(Statement statement)
    {
        var boundStatement = statement.Accept(this);
        _boundNodes.TryAdd(statement, boundStatement);
        return boundStatement;
    }

    private BoundExpression Bind(Expression expression)
    {
        var boundExpression = expression.Accept(this);
        _boundNodes.TryAdd(expression, boundExpression);
        return boundExpression;
    }
}