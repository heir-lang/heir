using Heir.AST;
using Heir.AST.Abstract;
using Heir.Binding;
using Heir.BoundAST;
using Heir.BoundAST.Abstract;
using Heir.Diagnostics;
using Heir.Runtime.Intrinsics;
using Heir.Syntax;
using Heir.Types;
using FunctionType = Heir.Types.FunctionType;
using IntersectionType = Heir.AST.IntersectionType;
using ParenthesizedType = Heir.Types.ParenthesizedType;
using UnionType = Heir.Types.UnionType;

namespace Heir;

using PropertyPair = KeyValuePair<LiteralType, InterfaceMemberSignature>;

public sealed class Binder(DiagnosticBag diagnostics, SyntaxTree syntaxTree)
    : Statement.IVisitor<BoundStatement>,
      Expression.IVisitor<BoundExpression>
{
    public SyntaxTree SyntaxTree { get; } = syntaxTree;

    private readonly Dictionary<SyntaxNode, BoundSyntaxNode> _boundNodes = [];
    private readonly Stack<Stack<VariableSymbol<BaseType>>> _variableScopes = [];
    private readonly Stack<Stack<TypeSymbol>> _typeScopes = [];

    public BoundSyntaxTree Bind()
    {
        EndScope();
        BeginScope();
        Intrinsics.RegisterGlobalSymbols(this);
        var tree = (BoundSyntaxTree)Bind(SyntaxTree);

        return tree;
    }

    public BoundSyntaxTree GetBoundSyntaxTree() => (BoundSyntaxTree)GetBoundNode(SyntaxTree);
    public BoundStatement GetBoundNode(Statement statement) => (BoundStatement)_boundNodes[statement];
    public BoundExpression GetBoundNode(Expression expression) => (BoundExpression)_boundNodes[expression];
    public BoundSyntaxNode GetBoundNode(SyntaxNode node) => _boundNodes[node];
    
    public TypeSymbol DefineTypeSymbol(Token name, BaseType type, bool isIntrinsic = false)
    {
        var symbol = new TypeSymbol(name, type, isIntrinsic);
        if (_typeScopes.TryPeek(out var scope))
            scope.Push(symbol);

        return symbol;
    }
    
    public TypeSymbol? FindTypeSymbol(Token name, bool errorIfNotFound = true)
    {
        var symbol = _typeScopes
            .SelectMany(v => v)
            .FirstOrDefault(symbol => symbol.Name.Text == name.Text);
        
        if (symbol != null)
            return symbol;

        if (errorIfNotFound)
            diagnostics.Error(DiagnosticCode.H005, $"Failed to find type symbol for type '{name.Text}'", name);
        
        return null;
    }
    
    public VariableSymbol<BaseType> DefineVariableSymbol(Token name, BaseType type, bool isMutable) =>
        DefineVariableSymbol<BaseType>(name, type, isMutable);

    public VariableSymbol<TType> DefineVariableSymbol<TType>(Token name, TType type, bool isMutable) where TType : BaseType
    {
        // this is so braindead
        var symbol = new VariableSymbol<TType>(name, type, isMutable);
        if (_variableScopes.TryPeek(out var scope))
            scope.Push(new VariableSymbol<BaseType>(name, type, isMutable));

        return symbol;
    }

    public VariableSymbol<BaseType>? FindVariableSymbol(Token name, bool errorIfNotFound = true)
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

    public BoundStatement VisitInterfaceField(InterfaceField interfaceField) => new BoundNoOpStatement();

    public BoundStatement VisitInterfaceDeclaration(InterfaceDeclaration interfaceDeclaration)
    {
        var type = new InterfaceType(
            interfaceDeclaration.Fields
                .ConvertAll(field => new PropertyPair(
                    new(field.Identifier.Text),
                    new(BaseType.FromTypeRef(field.Type), field.IsMutable)))
                .ToDictionary(),
            [],
            interfaceDeclaration.Identifier.Text
        );
        
        DefineTypeSymbol(interfaceDeclaration.Identifier, type);
        foreach (var field in interfaceDeclaration.Fields)
            Bind(field);
        
        return new BoundNoOpStatement();
    }
    
    public BoundStatement VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var initializer = variableDeclaration.Initializer != null ? Bind(variableDeclaration.Initializer) : null;
        BaseType type;
        if (variableDeclaration.Type != null)
        {
            var typeRef = Bind(variableDeclaration.Type);
            type = typeRef.Type;
        }
        else
            type = initializer?.Type ?? IntrinsicTypes.Any;

        if (variableDeclaration.Type == null && variableDeclaration.IsMutable && type is LiteralType literalType)
            type = literalType.AsPrimitive();

        var symbol = DefineVariableSymbol(variableDeclaration.Name.Token, type, variableDeclaration.IsMutable);
        return new BoundVariableDeclaration(symbol, initializer, variableDeclaration.IsMutable);
    }

    public BoundStatement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var boundParameters = functionDeclaration.Parameters
            .ConvertAll(Bind)
            .OfType<BoundParameter>()
            .ToList();

        BaseType? returnType = null;
        if (functionDeclaration.ReturnType != null)
        {
            var typeRef = Bind(functionDeclaration.ReturnType);
            returnType = typeRef.Type;
        }
        
        var parameterTypePairs = boundParameters.ConvertAll(parameter =>
            new KeyValuePair<string, BaseType>(parameter.Symbol.Name.Text, parameter.Initializer != null
                ? BaseType.Nullable(parameter.Type)
                : parameter.Type));
        
        // TODO: fix this!
        // if heir is left to infer a function return type, then we do not know the
        // return type of a recursive call within the function.
        // in this example, the type of `x` is any:
        // fn abc { let x = abc(); return 123; }
        var parameterTypes = new Dictionary<string, BaseType>(parameterTypePairs);
        var defaults = new Dictionary<string, object?>(boundParameters.ConvertAll(parameter =>
            new KeyValuePair<string, object?>(parameter.Symbol.Name.Text, parameter.Initializer?.Token.Value)
        ));
        
        var placeholderType = new Types.FunctionType(
            defaults,
            parameterTypes,
            returnType ?? IntrinsicTypes.Any
        );
        
        var placeholderSymbol = DefineVariableSymbol<BaseType>(functionDeclaration.Name.Token, placeholderType, false);
        var boundBody = (BoundBlock)Bind(functionDeclaration.Body);
        var finalType = new Types.FunctionType(
            defaults,
            parameterTypes,
            returnType ?? boundBody.Type
        );

        UndefineSymbol(placeholderSymbol);
        var symbol = DefineVariableSymbol(functionDeclaration.Name.Token, finalType, false);
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
    
    public BoundStatement VisitWhileStatement(While @while)
    {
        var condition = Bind(@while.Condition);
        var body = Bind(@while.Body);
        return new BoundWhile(@while.Keyword, condition, body);
    }

    public BoundStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = Bind(expressionStatement.Expression);
        return new BoundExpressionStatement(expression);
    }
    
    public BoundExpression VisitParameter(Parameter parameter)
    {
        var initializer = parameter.Initializer != null
            ? (BoundLiteral)Bind(parameter.Initializer)
            : null;
        
        var type = parameter.Type != null
            ? Bind(parameter.Type).Type
            : initializer != null ? initializer.Type : IntrinsicTypes.Any;
        
        if (parameter.Type == null && type is LiteralType literalType)
            type = literalType.AsPrimitive();

        var symbol = DefineVariableSymbol(parameter.Name.Token, type, true);
        return new BoundParameter(symbol, initializer);
    }

    public BoundExpression VisitMemberAccessExpression(MemberAccess memberAccess)
    {
        var expression = Bind(memberAccess.Expression);
        var name = new BoundIdentifierName(new VariableSymbol<BaseType>(memberAccess.Name.Token, IntrinsicTypes.Any, false));
        return new BoundMemberAccess(expression, name);
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
        
        if (binary.Left is BoundIdentifierName { Symbol: { IsMutable: false } symbol })
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
        var symbol = FindVariableSymbol(identifierName.Token) ??
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
            var boundValue = Bind(pair.Value);
            properties.Add(boundKey.Type, boundValue);
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
    public BoundExpression VisitNameOfExpression(NameOf nameOf) => new BoundNoOp();

    public BoundExpression VisitSingularTypeRef(AST.SingularType singularType)
    {
        var typeSymbol = FindTypeSymbol(singularType.Token);
        var type = typeSymbol?.Type ?? BaseType.FromTypeRef(singularType);
        return new BoundNoOp(type);
    }
    
    public BoundExpression VisitParenthesizedTypeRef(AST.ParenthesizedType parenthesizedType)
    {
        var type = new ParenthesizedType(Bind(parenthesizedType.Type).Type);
        return new BoundNoOp(type);
    }
    
    public BoundExpression VisitUnionTypeRef(AST.UnionType unionType)
    {
        var types = unionType.Types.Select(typeRef => Bind(typeRef).Type).ToList();
        var type = new UnionType(types);
        
        return new BoundNoOp(type);
    }
    
    public BoundExpression VisitIntersectionTypeRef(IntersectionType intersectionType)
    {
        var types = intersectionType.Types.Select(typeRef => Bind(typeRef).Type).ToList();
        var type = new Types.IntersectionType(types);
        
        return new BoundNoOp(type);
    }
    
    public BoundExpression VisitFunctionTypeRef(AST.FunctionType functionType)
    {
        var parameterTypes = functionType.ParameterTypes
            .Select(pair => new KeyValuePair<string, BaseType>(pair.Key, Bind(pair.Value).Type))
            .ToDictionary();
        
        var returnType = Bind(functionType.ReturnType).Type;
        var type = new FunctionType([], parameterTypes, returnType);
        return new BoundNoOp(type);
    }

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

    private void BeginScope()
    {
        _variableScopes.Push([]);
        _typeScopes.Push([]);
    }

    private void EndScope()
    {
        _variableScopes.TryPop(out _);
        _typeScopes.TryPop(out _);
    }

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