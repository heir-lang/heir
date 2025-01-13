using Heir.Binding;
using Heir.BoundAST;
using Heir.Syntax;
using Heir.Types;

namespace Heir;

public class TypeChecker(DiagnosticBag diagnostics, BoundSyntaxTree syntaxTree) : BoundStatement.Visitor<object?>, BoundExpression.Visitor<object?>
{
    public void Check() => Check(syntaxTree);

    public object? VisitBoundSyntaxTree(BoundSyntaxTree tree) => VisitBoundBlock(tree);
    public object? VisitBoundBlock(BoundBlock block) => Check(block.Statements);
    public object? VisitBoundReturnStatement(BoundReturn @return) => Check(@return.Expression);
    public object? VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => Check(expressionStatement.Expression);

    public object? VisitBoundFunctionDeclaration(BoundFunctionDeclaration declaration)
    {
        Check(declaration.Parameters.OfType<BoundExpression>().ToList());
        Check(declaration.Body);
        if (declaration.Body.Type.IsAssignableTo(declaration.Type.ReturnType))
            return null;
        
        var message = $"Function '{declaration.Symbol.Name.Text}' is expected to return type '{declaration.Type.ReturnType.ToString()}', but returns '{declaration.Body.Type.ToString()}'";
        diagnostics.Error(DiagnosticCode.H007, message, declaration.Symbol.Name);
        return null;
    }

    public object? VisitBoundVariableDeclaration(BoundVariableDeclaration variableDeclaration)
    {
        if (variableDeclaration.Initializer == null)
            return null;
        
        Check(variableDeclaration.Initializer);
        Assert(variableDeclaration.Initializer, variableDeclaration.Symbol.Type);
        return null;
    }
    
    public object? VisitBoundParameter(BoundParameter parameter)
    {
        if (parameter.Initializer == null)
            return null;
        
        Check(parameter.Initializer);
        Assert(parameter.Initializer, parameter.Symbol.Type);
        return null;
    }
    
    public object? VisitBoundAssignmentOpExpression(BoundAssignmentOp assignmentOp)
    {
        Check(assignmentOp.Right);
        Assert(assignmentOp.Right, assignmentOp.Left.Type);
        return null;
    }

    public object? VisitBoundBinaryOpExpression(BoundBinaryOp binaryOp)
    {
        Check(binaryOp.Left);
        Check(binaryOp.Right);
        Assert(binaryOp.Left, binaryOp.Operator.LeftType);
        Assert(binaryOp.Right, binaryOp.Operator.RightType);
        return null;
    }
    
    public object? VisitBoundLiteralExpression(BoundLiteral literal) => null;
    public object? VisitBoundObjectLiteralExpression(BoundObjectLiteral objectLiteral)
    {
        foreach (var property in objectLiteral.Properties)
        {
            Check(property.Value);
            if (property.Key is LiteralType literalType)
            {
                var signature = GetInterfaceMemberSignature(objectLiteral.Type, literalType, objectLiteral.Token);
                if (signature == null) continue;
                Assert(property.Value, signature.ValueType);
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
        return null;
    }

    public object? VisitBoundIdentifierNameExpression(BoundIdentifierName identifierName) => null;
    public object? VisitBoundNoOp(BoundNoOp noOp) => null;
    public object? VisitBoundNoOp(BoundNoOpStatement noOp) => null;

    public object? VisitBoundParenthesizedExpression(BoundParenthesized parenthesized) => Check(parenthesized.Expression);

    public object? VisitBoundUnaryOpExpression(BoundUnaryOp unaryOp)
    {
        Check(unaryOp.Operand);
        Assert(unaryOp.Operand, unaryOp.Operator.OperandType);
        return null;
    }
    
    private InterfaceMemberSignature? GetInterfaceMemberSignature(InterfaceType interfaceType, LiteralType propertyName, Token token)
    {
        if (!interfaceType.Members.TryGetValue(propertyName, out var valueType))
        {
            diagnostics.Error(DiagnosticCode.H013, $"Property '{propertyName.Value}' does not exist on '${interfaceType.Name}'", token);
            return null;
        }

        return valueType;
    }

    private object? Check(List<BoundStatement> nodes)
    {
        foreach (var node in nodes)
            Check(node);
        
        return null;
    }
    
    private object? Check(List<BoundExpression> nodes)
    {
        foreach (var node in nodes)
            Check(node);
        
        return null;
    }
    
    private object? Check(List<BoundSyntaxNode> nodes)
    {
        foreach (var node in nodes)
            Check(node);
        
        return null;
    }
    
    private object? Check(BoundExpression expression) => expression.Accept(this);
    private object? Check(BoundStatement statement) => statement.Accept(this);
    private object? Check(BoundSyntaxNode node)
    {
        if (node is BoundExpression expression)
            Check(expression);
        else if (node is BoundStatement statement)
            Check(statement);

        return null;
    }

    private void Assert(BoundExpression node, BaseType type, string? message = null)
    {
        if (node.Type.IsAssignableTo(type)) return;
        diagnostics.Error(DiagnosticCode.H007, message ?? $"Type '{node.Type.ToString()}' is not assignable to type '{type.ToString()}'", node.GetFirstToken());
    }
}