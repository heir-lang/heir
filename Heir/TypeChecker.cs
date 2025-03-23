using Heir.Binding;
using Heir.BoundAST;
using Heir.BoundAST.Abstract;
using Heir.Diagnostics;
using Heir.Syntax;
using Heir.Types;
using Void = Heir.BoundAST.Abstract.Void;

namespace Heir;

public class TypeChecker(DiagnosticBag diagnostics, BoundSyntaxTree syntaxTree) : IBoundNodeVisitor
{
    public void Check() => Check(syntaxTree);

    public Void VisitBoundSyntaxTree(BoundSyntaxTree tree) => VisitBoundBlock(tree);
    public Void VisitBoundBlock(BoundBlock block) => Check(block.Statements);
    public Void VisitBoundReturnStatement(BoundReturn @return) => Check(@return.Expression);
    public Void VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => Check(expressionStatement.Expression);
    public Void VisitBoundBreakStatement(BoundBreak @break) => default;
    public Void VisitBoundContinueStatement(BoundContinue @continue) => default;

    public Void VisitBoundFunctionDeclaration(BoundFunctionDeclaration declaration)
    {
        Check(declaration.Parameters.OfType<BoundExpression>().ToList());
        Check(declaration.Body);
        if (declaration.Body.Type.IsAssignableTo(declaration.Type.ReturnType))
            return default;
        
        var message = $"Function '{declaration.Symbol.Name.Text}' is expected to return type '{declaration.Type.ReturnType.ToString()}', but returns '{declaration.Body.Type.ToString()}'";
        diagnostics.Error(DiagnosticCode.H007, message, declaration.Symbol.Name);
        return default;
    }

    public Void VisitBoundVariableDeclaration(BoundVariableDeclaration variableDeclaration)
    {
        if (variableDeclaration.Initializer == null)
            return default;
        
        Check(variableDeclaration.Initializer);
        Assert(variableDeclaration.Initializer, variableDeclaration.Symbol.Type);
        return default;
    }

    public Void VisitBoundEnumDeclaration(BoundEnumDeclaration enumDeclaration)
    {
        foreach (var member in enumDeclaration.Members)
            Check(member);
        
        return default;
    }

    public Void VisitBoundEnumMember(BoundEnumMember enumMember)
    {
        Check(enumMember.Value);
        return default;
    }

    public Void VisitBoundIfStatement(BoundIf @if)
    {
        Check(@if.Condition);
        Check(@if.Body);
        if (@if.ElseBranch == null)
            return default;
        
        Check(@if.ElseBranch);
        return default;
    }
    
    public Void VisitBoundWhileStatement(BoundWhile @while)
    {
        Check(@while.Condition);
        Check(@while.Body);
        return default;
    }

    public Void VisitBoundParameter(BoundParameter parameter)
    {
        if (parameter.Initializer == null)
            return default;
        
        Check(parameter.Initializer);
        Assert(parameter.Initializer, parameter.Symbol.Type);
        return default;
    }
    
    public Void VisitBoundMemberAccessExpression(BoundMemberAccess memberAccess)
    {
        Check(memberAccess.Expression);
        Check(memberAccess.Name);
        
        // TODO: in the future check for indexable literal types (like strings), and also arrays
        if (memberAccess.Expression.Type is not InterfaceType interfaceType)
        {
            diagnostics.Error(DiagnosticCode.H018, $"Attempt to index '{memberAccess.Expression.Type.ToString()}'", memberAccess.Expression);
            return default;
        }
        
        if (memberAccess.Type == memberAccess.Expression.Type)
        {
            diagnostics.Error(DiagnosticCode.H013, $"No member '{memberAccess.Name.Symbol.Name.Text}' exists on type '{interfaceType.Name}'", memberAccess.Name);
            return default;
        }
        
        return default;
    }

    public Void VisitBoundElementAccessExpression(BoundElementAccess elementAccess)
    {
        Check(elementAccess.Expression);
        Check(elementAccess.IndexExpression);
        
        // TODO: in the future check for indexable literal types (like strings), and also arrays
        if (elementAccess.Expression.Type is not InterfaceType interfaceType)
        {
            diagnostics.Error(DiagnosticCode.H018, $"Attempt to index '{elementAccess.Expression.Type.ToString()}'", elementAccess.Expression);
            return default;
        }
        
        Assert(elementAccess.IndexExpression, interfaceType.IndexType);
        return default;
    }

    public Void VisitBoundInvocationExpression(BoundInvocation invocation)
    {
        Check(invocation.Callee);
        if (BaseType.UnwrapParentheses(invocation.Callee.Type) is not FunctionType functionType)
        {
            diagnostics.Error(DiagnosticCode.H018, $"Attempt to call value of type '{invocation.Callee.Type.ToString()}'", invocation.Callee);
            return default;
        }

        var argumentCount = invocation.Arguments.Count;
        var minimumArguments = functionType.Arity.Start.Value;
        var maximumArguments = functionType.Arity.End.Value;
        if (argumentCount < minimumArguments || argumentCount > maximumArguments)
        {
            var argumentCountDisplay = minimumArguments == maximumArguments
                ? minimumArguments.ToString()
                : minimumArguments + "-" + maximumArguments;
            
            diagnostics.Error(DiagnosticCode.H019, $"Expected {argumentCountDisplay} argument{(maximumArguments != 1 ? "s" : "")}, got {argumentCount}", invocation.Callee);
            return default;
        }
            
        var expectedTypes = functionType.ParameterTypes.ToList();
        var index = 0;
        foreach (var argument in invocation.Arguments)
        {
            var parameterTypeInfo = expectedTypes.ElementAtOrDefault(index++);
            var defaultPair = default(KeyValuePair<string, BaseType>);
            if (parameterTypeInfo.Key == defaultPair.Key && parameterTypeInfo.Value == defaultPair.Value) continue;
            
            var (parameterName, expectedType) = parameterTypeInfo;
            Check(argument);
            Assert(argument, expectedType, $"Argument type '{argument.Type.ToString()}' is not assignable to type '{expectedType.ToString()}' of parameter '{parameterName}'");
        }

        return default;
    }

    public Void VisitBoundAssignmentOpExpression(BoundAssignmentOp assignmentOp)
    {
        Check(assignmentOp.Right);
        Assert(assignmentOp.Right, assignmentOp.Left.Type);
        return default;
    }

    public Void VisitBoundBinaryOpExpression(BoundBinaryOp binaryOp)
    {
        Check(binaryOp.Left);
        Check(binaryOp.Right);
        Assert(binaryOp.Left, binaryOp.Operator.LeftType);
        Assert(binaryOp.Right, binaryOp.Operator.RightType);
        return default;
    }
    
    public Void VisitBoundLiteralExpression(BoundLiteral literal) => default;
    public Void VisitBoundObjectLiteralExpression(BoundObjectLiteral objectLiteral)
    {
        foreach (var property in objectLiteral.Properties)
        {
            Check(property.Value);
            if (property.Key is LiteralType literalType)
            {
                var signature = GetInterfaceMemberSignature(objectLiteral.Type, literalType, objectLiteral.Token);
                if (signature == null) continue;
                Assert(property.Value, signature.Type);
            }
            else
            {
                if (property.Key.IsAssignableTo(IntrinsicTypes.Index) && objectLiteral.Type.IndexSignatures.TryGetValue((PrimitiveType)property.Key, out var valueType))
                {
                    Assert(property.Value, valueType);
                    continue;
                }

                // pretty sure this check is quite literally useless
                diagnostics.Error(DiagnosticCode.H013, $"Index signature for '{property.Key.ToString()}' does not exist on '{objectLiteral.Type.Name}'", objectLiteral.Token);
            }
        }
        return default;
    }

    public Void VisitBoundArrayLiteralExpression(BoundArrayLiteral arrayLiteral)
    {
        foreach (var element in arrayLiteral.Elements)
            Check(element);
        
        return default;
    }

    public Void VisitBoundIdentifierNameExpression(BoundIdentifierName identifierName) => default;
    public Void VisitBoundNoOp(BoundNoOp noOp) => default;
    public Void VisitBoundNoOp(BoundNoOpStatement noOp) => default;

    public Void VisitBoundParenthesizedExpression(BoundParenthesized parenthesized) => Check(parenthesized.Expression);

    public Void VisitBoundUnaryOpExpression(BoundUnaryOp unaryOp)
    {
        Check(unaryOp.Operand);
        Assert(unaryOp.Operand, unaryOp.Operator.OperandType);
        return default;
    }
    
    public Void VisitBoundPostfixOpExpression(BoundPostfixOp postfixOp)
    {
        Check(postfixOp.Operand);
        Assert(postfixOp.Operand, postfixOp.Operator.OperandType);
        return default;
    }
    
    private InterfaceMemberSignature? GetInterfaceMemberSignature(InterfaceType interfaceType, LiteralType propertyName, Token token)
    {
        if (interfaceType.Members.TryGetValue(propertyName, out var valueType))
            return valueType;
        
        diagnostics.Error(DiagnosticCode.H013, $"Property '{propertyName.Value}' does not exist on '${interfaceType.Name}'", token);
        return null;
    }

    private Void Check(List<BoundStatement> nodes)
    {
        foreach (var node in nodes)
            Check(node);
        
        return default;
    }
    
    private Void Check(List<BoundExpression> nodes)
    {
        foreach (var node in nodes)
            Check(node);
        
        return default;
    }
    
    private Void Check(List<BoundSyntaxNode> nodes)
    {
        foreach (var node in nodes)
            Check(node);
        
        return default;
    }
    
    private Void Check(BoundExpression expression) => expression.Accept(this);
    private Void Check(BoundStatement statement) => statement.Accept(this);
    private Void Check(BoundSyntaxNode node)
    {
        if (node is BoundExpression expression)
            Check(expression);
        else if (node is BoundStatement statement)
            Check(statement);

        return default;
    }

    private void Assert(BoundExpression node, BaseType type, string? message = null)
    {
        if (node.Type.IsAssignableTo(type)) return;
        diagnostics.Error(DiagnosticCode.H007, message ?? $"Type '{node.Type.ToString()}' is not assignable to type '{type.ToString()}'", node.GetFirstToken());
    }
}