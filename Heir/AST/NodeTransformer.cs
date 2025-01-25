using Heir.AST.Abstract;

namespace Heir.AST;

public abstract class NodeTransformer(SyntaxTree tree) : INodeVisitor<SyntaxNode?>
{
    protected SyntaxTree CleanTree { get; } = tree;
    
    protected SyntaxTree Transform() => (SyntaxTree)Transform(CleanTree)!;
    
    public virtual SyntaxNode? VisitSyntaxTree(SyntaxTree tree)
    {
        var newStatements = (from statement in tree.Statements
            let transformedStatement = Transform(statement)
            select transformedStatement ?? statement).ToList();

        return new SyntaxTree(newStatements, tree.Diagnostics);
    }
    
    public virtual SyntaxNode? VisitIdentifierNameExpression(IdentifierName identifierName) => null;

    public virtual SyntaxNode? VisitAssignmentOpExpression(AssignmentOp assignmentOp)
    {
        var left = Transform(assignmentOp.Left) as AssignmentTarget;
        var right = Transform(assignmentOp.Right);
        if (left == null && right == null)
            return null;

        var newNode = assignmentOp;
        if (left != null)
            newNode = newNode.WithLeft(left);
        if (right != null)
            newNode = newNode.WithRight(right);
        
        return newNode;
    }
    
    public virtual SyntaxNode? VisitUnaryOpExpression(UnaryOp unaryOp)
    {
        var operand = Transform(unaryOp.Operand);
        return operand == null 
            ? null
            : unaryOp.WithOperand(operand);
    }
    
    public virtual SyntaxNode? VisitBinaryOpExpression(BinaryOp binaryOp)
    {
        var left = Transform(binaryOp.Left);
        var right = Transform(binaryOp.Right);
        if (left == null && right == null)
            return null;

        var newNode = binaryOp;
        if (left != null)
            newNode = newNode.WithLeft(left);
        if (right != null)
            newNode = newNode.WithRight(right);
        
        return newNode;
    }

    public virtual SyntaxNode? VisitParenthesizedExpression(Parenthesized parenthesized)
    {
        var expression = Transform(parenthesized.Expression);
        return expression == null
            ? null
            : new Parenthesized(expression);
    }
    
    public virtual SyntaxNode? VisitLiteralExpression(Literal literal) => null;

    public virtual SyntaxNode? VisitObjectLiteralExpression(ObjectLiteral objectLiteral)
    {
        var properties = objectLiteral.Properties
            .Select(property =>
            {
                var pair = new KeyValuePair<Expression, Expression>(
                    Transform(property.Key) ?? property.Key,
                    Transform(property.Value) ?? property.Value);
                
                return pair;
            })
            .ToDictionary();
        
        return new ObjectLiteral(objectLiteral.Token, properties);
    }
    
    public virtual SyntaxNode? VisitNoOp(NoOp noOp) => null;
    public virtual SyntaxNode? VisitNoOp(NoOpType noOp) => null;
    public virtual SyntaxNode? VisitNoOp(NoOpStatement noOp) => null;
    public virtual SyntaxNode? VisitSingularTypeRef(SingularType singularType) => null;
    public virtual SyntaxNode? VisitParenthesizedTypeRef(ParenthesizedType parenthesizedType) => null;
    public virtual SyntaxNode? VisitUnionTypeRef(UnionType unionType) => null;
    public virtual SyntaxNode? VisitIntersectionTypeRef(IntersectionType intersectionType) => null;
    public virtual SyntaxNode? VisitFunctionTypeRef(FunctionType functionType) => null;
    public virtual SyntaxNode? VisitParameter(Parameter parameter)
    {
        var name = Transform(parameter.Name) as IdentifierName;
        var initializer = parameter.Initializer != null ? Transform(parameter.Initializer) as Literal : null;
        var typeRef = parameter.Type != null ? Transform(parameter.Type) as TypeRef : null;
        if (name == null && initializer == null && typeRef == null)
            return null;

        var newNode = parameter;
        if (name != null)
            newNode = newNode.WithName(name);
        if (initializer != null)
            newNode = newNode.WithInitializer(initializer);
        if (typeRef != null)
            newNode = newNode.WithType(typeRef);
        
        return newNode;
    }

    public virtual SyntaxNode? VisitInvocationExpression(Invocation invocation)
    {
        var callee = Transform(invocation.Callee);
        var arguments = invocation.Arguments.ConvertAll(argument => Transform(argument) ?? argument);
        return new Invocation(callee ?? invocation.Callee, arguments);
    }
    
    public virtual SyntaxNode? VisitElementAccessExpression(ElementAccess elementAccess)
    {
        var expression = Transform(elementAccess.Expression);
        var indexExpression = Transform(elementAccess.IndexExpression);
        if (expression == null && indexExpression == null)
            return null;
        
        return new ElementAccess(expression ?? elementAccess.Expression, indexExpression ?? elementAccess.IndexExpression);
    }
    
    public virtual SyntaxNode? VisitMemberAccessExpression(MemberAccess memberAccess)
    {
        var expression = Transform(memberAccess.Expression);
        var name = Transform(memberAccess.Name) as IdentifierName;
        if (expression == null && name == null)
            return null;
        
        return new MemberAccess(expression ?? memberAccess.Expression, name ?? memberAccess.Name);
    }

    public virtual SyntaxNode? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var name = Transform(variableDeclaration.Name) as IdentifierName;
        var initializer = variableDeclaration.Initializer != null ? Transform(variableDeclaration.Initializer) : null;
        var typeRef = variableDeclaration.Type != null ? Transform(variableDeclaration.Type) as TypeRef : null;
        if (name == null && initializer == null && typeRef == null)
            return null;
        
        return new VariableDeclaration(
            name ?? variableDeclaration.Name,
            initializer ?? variableDeclaration.Initializer,
            typeRef ?? variableDeclaration.Type,
            variableDeclaration.IsMutable);
    }

    public virtual SyntaxNode? VisitBlock(Block block)
    {
        var newStatements = (from statement in block.Statements
            let transformedStatement = Transform(statement)
            select transformedStatement ?? statement).ToList();

        return new Block(newStatements); 
    }

    public virtual SyntaxNode? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = Transform(expressionStatement.Expression);
        return expression == null
            ? null
            : new ExpressionStatement(expression);
    }
    
    public virtual SyntaxNode? VisitReturnStatement(Return @return)
    {
        var expression = Transform(@return.Expression);
        return expression == null
            ? null
            : new Return(@return.Keyword, expression);
    }
    
    public virtual SyntaxNode? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var name = Transform(functionDeclaration.Name) as IdentifierName;
        var body = Transform(functionDeclaration.Body) as Block;
        var returnTypeRef = functionDeclaration.ReturnType != null
            ? Transform(functionDeclaration.ReturnType) as TypeRef
            : null;
        var parameters = functionDeclaration.Parameters
            .ConvertAll(parameter => Transform(parameter) ?? parameter)
            .OfType<Parameter>()
            .ToList();
        
        return new FunctionDeclaration(
            functionDeclaration.Keyword,
            name ?? functionDeclaration.Name,
            parameters,
            body ?? functionDeclaration.Body,
            returnTypeRef ?? functionDeclaration.ReturnType);
    }

    public virtual SyntaxNode? VisitIfStatement(If @if)
    {
        var condition = Transform(@if.Condition);
        var body = Transform(@if.Body);
        var elseBranch = @if.ElseBranch != null ? Transform(@if.ElseBranch) : null;
        if (condition == null && body == null && elseBranch == null)
            return null;

        return new If(
            @if.Keyword,
            condition ?? @if.Condition,
            body ?? @if.Body,
            elseBranch ?? @if.ElseBranch);
    }

    public virtual SyntaxNode? VisitInterfaceField(InterfaceField interfaceField)
    {
        return Transform(interfaceField.Type) is not TypeRef typeRef
            ? null
            : new InterfaceField(interfaceField.Identifier, typeRef, interfaceField.IsMutable);
    }

    public virtual SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclaration interfaceDeclaration)
    {
        var fields = interfaceDeclaration.Fields
            .ConvertAll(field => Transform(field) ?? field)
            .OfType<InterfaceField>()
            .ToList();
        
        return new InterfaceDeclaration(interfaceDeclaration.Keyword, interfaceDeclaration.Identifier, fields);
    }

    public virtual SyntaxNode? VisitNameOfExpression(NameOf nameOf)
    {
        return Transform(nameOf.Name) is not IdentifierName name
            ? null
            : new NameOf(nameOf.Keyword, name);
    }
    
    protected Expression? Transform(Expression expression) => expression.Accept(this) as Expression;
    protected Statement? Transform(Statement statement) => statement.Accept(this) as Statement;
    protected SyntaxNode? Transform(SyntaxNode node) =>
        node switch
        {
            Expression expression => Transform(expression),
            Statement statement => Transform(statement),
            _ => null!
        };
}