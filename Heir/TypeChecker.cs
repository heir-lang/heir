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

    public Void VisitBoundTypeParameter(BoundTypeParameter boundTypeParameter) => default;

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
        
        if (!HandleGenericArguments(invocation, functionType))
            return default;

        if (!CheckArgumentCount(invocation, functionType))
            return default;
            
        CheckArguments(invocation, functionType);
        return default;
    }
    
    /// <summary>
    /// Handles generic type arguments for the invocation, either explicitly provided or by inference.
    /// </summary>
    private bool HandleGenericArguments(BoundInvocation invocation, FunctionType functionType)
    {
        if (functionType.TypeParameters.Count == 0)
            return true;
    
        var expectedCount = functionType.TypeParameters.Count;
        var providedCount = invocation.TypeArguments.Count;
        return providedCount > 0
            ? HandleExplicitTypeArguments(invocation, functionType, expectedCount, providedCount)
            : HandleInferredTypeArguments(invocation, functionType, expectedCount);
    }
    
    /// <summary>
    /// Processes explicit type arguments ensuring they satisfy constraints and fills in defaults using initializers.
    /// </summary>
    private bool HandleExplicitTypeArguments(BoundInvocation invocation, FunctionType functionType, int expectedCount, int providedCount)
    {
        if (providedCount > expectedCount)
        {
            diagnostics.Error(DiagnosticCode.H025,
                $"Expected at most {expectedCount} generic type argument{(expectedCount != 1 ? "s" : "")}, got {providedCount}",
                invocation.Callee);
            
            return false;
        }
    
        // handle constraints
        for (var i = 0; i < providedCount; i++)
        {
            var typeParameter = functionType.TypeParameters[i];
            var typeArgument = invocation.TypeArguments[i];
            if (typeParameter.BaseType == null) continue;
            if (typeArgument.IsAssignableTo(typeParameter)) continue;
            
            diagnostics.Error(DiagnosticCode.H025,
                $"Type '{typeArgument.ToString()}' does not satisfy the constraint '{typeParameter.BaseType.ToString()}' for generic parameter '{typeParameter.Name}'",
                invocation.Callee);
            
            return false;
        }
    
        // fill in missing type arguments using initializers
        for (var i = providedCount; i < expectedCount; i++)
        {
            var typeParameter = functionType.TypeParameters[i];
            if (typeParameter.Initializer != null)
                invocation.TypeArguments.Add(typeParameter.Initializer);
            else
            {
                diagnostics.Error(DiagnosticCode.H025,
                    $"Expected {expectedCount} generic type arguments, but only {providedCount} were provided and no default is available for parameter '{typeParameter.Name}'",
                    invocation.Callee);
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Attempts to infer generic type arguments from the invocation's arguments and fills in defaults if needed.
    /// </summary>
    private bool HandleInferredTypeArguments(BoundInvocation invocation, FunctionType functionType, int expectedCount)
    {
        var inferredTypeArguments = InferTypeArguments(functionType, invocation.Arguments);
        if (inferredTypeArguments == null)
        {
            // try individual inference or fallback to initializer
            inferredTypeArguments = [];
            foreach (var typeParameter in functionType.TypeParameters)
            {
                var inferred = InferFromContext(functionType, typeParameter, invocation.Arguments);
                if (inferred != null)
                    inferredTypeArguments.Add(inferred);
                else if (typeParameter.Initializer != null)
                    inferredTypeArguments.Add(typeParameter.Initializer);
                else
                {
                    diagnostics.Error(DiagnosticCode.H025,
                        $"Could not infer generic type argument for parameter '{typeParameter.Name}', and no default initializer is provided",
                        invocation.Callee);
                    return false;
                }
            }
        }
        else if (inferredTypeArguments.Count < expectedCount)
        {
            // fill in missing inferred arguments with defaults
            for (var i = inferredTypeArguments.Count; i < expectedCount; i++)
            {
                var typeParameter = functionType.TypeParameters[i];
                if (typeParameter.Initializer != null)
                    inferredTypeArguments.Add(typeParameter.Initializer);
                else
                {
                    diagnostics.Error(DiagnosticCode.H025,
                        $"Could not infer generic type argument for parameter '{typeParameter.Name}', and no default initializer is provided",
                        invocation.Callee);
                    return false;
                }
            }
        }
        
        return HandleGenericArguments(invocation.WithTypeArguments(inferredTypeArguments), functionType);
    }
    
    /// <summary>Checks that the invocation provides a valid number of arguments.</summary>
    private bool CheckArgumentCount(BoundInvocation invocation, FunctionType functionType)
    {
        var argumentCount = invocation.Arguments.Count;
        var minimumArguments = functionType.Arity.Start.Value;
        var maximumArguments = functionType.Arity.End.Value;
        if (argumentCount >= minimumArguments && argumentCount <= maximumArguments)
            return true;
        
        var argumentCountDisplay = minimumArguments == maximumArguments
            ? minimumArguments.ToString()
            : minimumArguments + "-" + maximumArguments;
        
        diagnostics.Error(DiagnosticCode.H019,
            $"Expected {argumentCountDisplay} argument{(maximumArguments != 1 ? "s" : "")}, got {argumentCount}",
            invocation.Callee);
        
        return false;
    }
    
    /// <summary>
    /// Checks each argument's type against the corresponding expected parameter type,
    /// after substituting generic type parameters with the provided type arguments.
    /// </summary>
    private void CheckArguments(BoundInvocation invocation, FunctionType functionType)
    {
        // substitution map from generic parameter names to concrete type arguments
        var substitutionMap = new Dictionary<string, BaseType>();
        for (var i = 0; i < functionType.TypeParameters.Count; i++)
        {
            var typeParameter = functionType.TypeParameters[i];
            substitutionMap[typeParameter.Name] = invocation.TypeArguments.ElementAtOrDefault(i) ?? typeParameter.BaseType ?? IntrinsicTypes.Any;
        }
    
        var expectedTypes = functionType.Parameters.ToList();
        var index = 0;
        foreach (var argument in invocation.Arguments)
        {
            var parameterTypeInfo = expectedTypes.ElementAtOrDefault(index++);
            var defaultPair = default(KeyValuePair<string, BaseType>);
            if (parameterTypeInfo.Key == defaultPair.Key && parameterTypeInfo.Value == defaultPair.Value)
                continue;
    
            var (parameterName, expectedType) = parameterTypeInfo;
            var concreteExpectedType = SubstituteGenericParameters(expectedType, substitutionMap);
    
            Check(argument);
            Assert(argument, concreteExpectedType,
                $"Argument type '{argument.Type.ToString()}' is not assignable to type '{concreteExpectedType.ToString()}' of parameter '{parameterName}'");
        }
    }
    
    /// <summary>
    /// If the given type is a generic type parameter, substitute it with the concrete type
    /// from the substitution map. For composite types, you might need to recursively substitute
    /// contained types. This simple implementation only handles direct matches.
    /// </summary>
    private static BaseType SubstituteGenericParameters(BaseType type, Dictionary<string, BaseType> substitutionMap)
    {
        if (type is TypeParameter tp && substitutionMap.TryGetValue(tp.Name, out var substituted))
            return substituted;
    
        // Extend here for composite types that may contain generic parameters
        return type;
    }
        
    /// <summary>
    /// Attempts to infer all generic type arguments for the function based on the provided arguments.
    /// Returns a list of inferred types in the same order as functionType.TypeParameters,
    /// or null if inference failed (for example, due to conflicting inferences).
    /// </summary>
    private static List<BaseType>? InferTypeArguments(FunctionType functionType, IList<BoundExpression> arguments)
    {
        var inferredMap = new Dictionary<string, BaseType>();
        var parametersList = functionType.Parameters.ToList();
    
        for (var i = 0; i < arguments.Count && i < parametersList.Count; i++)
        {
            var (_, paramType) = parametersList[i];
            var argType = arguments[i].Type;

            if (paramType is not SingularType type) continue;
            if (inferredMap.TryGetValue(type.Name, out var existing))
            {
                // if we already inferred a type, ensure the new one is consistent
                if (existing != argType)
                    return null; // conflict in inference; unable to infer a consistent type
            }
            else
                inferredMap[type.Name] = argType;
            // (For composite types containing generics, additional logic would be needed.)
        }
    
        var inferredList = new List<BaseType>();
        foreach (var typeParam in functionType.TypeParameters)
        {
            if (!inferredMap.TryGetValue(typeParam.Name, out var inferred)) continue;
            inferredList.Add(inferred);
        }
    
        // if we inferred at least one type parameter, return the list
        return inferredList.Count > 0 ? inferredList : null;
    }
    
    /// <summary>
    /// Attempts to infer a single generic type argument for a specific type parameter from the context.
    /// Searches for a parameter that is exactly the type parameter and returns the corresponding argument type.
    /// Returns null if no suitable context is found.
    /// </summary>
    private static BaseType? InferFromContext(FunctionType functionType, TypeParameter typeParam, IList<BoundExpression> arguments)
    {
        var parametersList = functionType.Parameters.ToList();
        
        for (var i = 0; i < parametersList.Count && i < arguments.Count; i++)
        {
            var (_, paramType) = parametersList[i];
            if (paramType is SingularType type && type.Name == typeParam.Name)
                return arguments[i].Type;
            
            // For composite parameter types that contain type parameters,
            // additional logic would be required to extract the type argument.
        }
        
        return null;
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