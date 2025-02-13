using Heir.Syntax;
using Heir.AST;
using Heir.AST.Abstract;
using Heir.Binding;
using Heir.BoundAST;
using Heir.CodeGeneration;
using Heir.Diagnostics;
using Heir.Types;
using FunctionType = Heir.AST.FunctionType;
using IntersectionType = Heir.AST.IntersectionType;
using ParenthesizedType = Heir.AST.ParenthesizedType;
using SingularType = Heir.AST.SingularType;
using UnionType = Heir.AST.UnionType;

namespace Heir;

public sealed class BytecodeGenerator(DiagnosticBag diagnostics, Binder binder) : INodeVisitor<List<Instruction>>
{
    private readonly SyntaxTree _syntaxTree = binder.SyntaxTree;
    
    public Bytecode GenerateBytecode()
    {
        var bytecode = GenerateBytecode(_syntaxTree);
        var optimizer = new BytecodeOptimizer(bytecode, diagnostics);
        var optimizedBytecode = optimizer.Optimize();

        return new(optimizedBytecode);
    }

    public List<Instruction> VisitSyntaxTree(SyntaxTree tree)
    {
        var statementsBytecode = GenerateStatementsBytecode(tree.Statements).ToList();
        if (statementsBytecode.LastOrDefault()?.OpCode != OpCode.RETURN)
            statementsBytecode.Add(new Instruction(tree, OpCode.EXIT));

        return statementsBytecode;
    }

    public List<Instruction> VisitBlock(Block block) =>
        GenerateStatementsBytecode(block.Statements)
            .Prepend(new Instruction(block, OpCode.BEGINSCOPE))
            .Append(new Instruction(block, OpCode.ENDSCOPE))
            .ToList();

    public List<Instruction> VisitInterfaceField(InterfaceField interfaceField) => NoOp(interfaceField);
    public List<Instruction> VisitInterfaceDeclaration(InterfaceDeclaration interfaceDeclaration) =>
        NoOp(interfaceDeclaration);

    public List<Instruction> VisitVariableDeclaration(VariableDeclaration variableDeclaration) =>
    [
        new(variableDeclaration.Name, OpCode.PUSH, variableDeclaration.Name.Token.Text),
        ..variableDeclaration.Initializer != null
            ? GenerateBytecode(variableDeclaration.Initializer)
            : [new(variableDeclaration, OpCode.PUSHNONE)],
        
        new(variableDeclaration, OpCode.STORE, false)
    ];

    public List<Instruction> VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var bodyBytecode = GenerateBytecode(functionDeclaration.Body);
        var hasExplicitReturn = bodyBytecode.Last().OpCode == OpCode.RETURN ||
                                bodyBytecode.SkipLast(1).Last().OpCode == OpCode.RETURN;
        if (!hasExplicitReturn)
            bodyBytecode.AddRange([
                new(functionDeclaration.Body, OpCode.PUSHNONE),
                new(functionDeclaration.Body, OpCode.RETURN)
            ]);

        return
        [
            new(functionDeclaration.Name, OpCode.PUSH, functionDeclaration.Name.Token.Text),
            new(functionDeclaration, OpCode.PROC, bodyBytecode),
            new(functionDeclaration, OpCode.STORE, false)
        ];
    }

    public List<Instruction> VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        if (enumDeclaration.IsInline)
            return NoOp(enumDeclaration);
        
        var objectValue = new Dictionary<List<Instruction>, List<Instruction>>(
            enumDeclaration.Members
                .ToList()
                .ConvertAll<KeyValuePair<List<Instruction>, List<Instruction>>>(member =>
                {
                    var keyExpression = new Literal(TokenFactory.StringFromIdentifier(member.Name.Token));
                    var key = GenerateBytecode(keyExpression);
                    var value = GenerateBytecode(member);
                    return new(key, value);
                })
        );

        return [
            ..PushName(enumDeclaration.Name),
            new Instruction(enumDeclaration, OpCode.PUSHOBJECT, objectValue),
            new Instruction(enumDeclaration, OpCode.STORE, false)
        ];
    }

    public List<Instruction> VisitEnumMember(EnumMember enumMember) => GenerateBytecode(enumMember.Value);

    public List<Instruction> VisitIfStatement(If @if)
    {
        var conditionBytecode = GenerateBytecode(@if.Condition);
        var bodyBytecode = GenerateBytecode(@if.Body);
        var bodyOptimizer = new BytecodeOptimizer(bodyBytecode, diagnostics);
        var optimizedBody = bodyOptimizer.Optimize(); // to calculate offset
        var elseBranchBytecode = @if.ElseBranch != null
            ? GenerateBytecode(@if.ElseBranch)
            : [];
        
        var elseBranchOptimizer = new BytecodeOptimizer(elseBranchBytecode, diagnostics);
        var optimizedElseBranch = elseBranchOptimizer.Optimize();
        return
        [
            ..conditionBytecode,
            new(@if, OpCode.JNZ, optimizedElseBranch.Count + 2),
            ..elseBranchBytecode,
            new(@if, OpCode.JMP, optimizedBody.Count + 1),
            ..bodyBytecode
        ];
    }
    
    public List<Instruction> VisitWhileStatement(While @while)
    {
        var conditionBytecode = GenerateBytecode(@while.Condition);
        var bodyBytecode = GenerateBytecode(@while.Body);
        var conditionOptimizer = new BytecodeOptimizer(conditionBytecode, diagnostics);
        var optimizedCondition = conditionOptimizer.Optimize(); // to calculate offset
        var bodyOptimizer = new BytecodeOptimizer(bodyBytecode, diagnostics);
        var optimizedBody = bodyOptimizer.Optimize();
        var filledInBody = FixLoopBody(bodyBytecode, optimizedBody.Count);

        return [
            ..conditionBytecode,
            new(@while, OpCode.JZ, optimizedBody.Count + 2),
            ..filledInBody,
            new(@while, OpCode.JMP, -optimizedBody.Count - optimizedCondition.Count - 1),
        ];
    }

    private static List<Instruction> FixLoopBody(List<Instruction> bodyBytecode, int totalLength)
    {
        return bodyBytecode.ConvertAll(instruction =>
        {
            if (instruction.OpCode is not OpCode.BREAK and not OpCode.CONTINUE)
                return instruction;

            var i = bodyBytecode.IndexOf(instruction);
            var offset = totalLength + 1 - i - (instruction.OpCode == OpCode.CONTINUE ? 1 : 0);
            return instruction
                .WithOpCode(OpCode.JMP)
                .WithOperand(offset);
        });
    }

    public List<Instruction> VisitParameter(Parameter parameter) =>
    [
        new(parameter.Name, OpCode.PUSH, parameter.Name.Token.Text),
        ..parameter.Initializer != null ? GenerateBytecode(parameter.Initializer) : [],
        new(parameter, OpCode.STORE, false)
    ];
    
    public List<Instruction> VisitMemberAccessExpression(MemberAccess memberAccess) =>
    [
        ..GenerateBytecode(memberAccess.Expression),
        ..PushName(memberAccess.Name),
        new(memberAccess, OpCode.INDEX)
    ];

    public List<Instruction> VisitElementAccessExpression(ElementAccess elementAccess) =>
    [
        ..GenerateBytecode(elementAccess.Expression),
        ..GenerateBytecode(elementAccess.IndexExpression),
        new(elementAccess, OpCode.INDEX)
    ];

    public List<Instruction> VisitInvocationExpression(Invocation invocation)
    {
        var boundInvocation = (BoundInvocation)binder.GetBoundNode(invocation);
        var argumentsBytecode = invocation.Arguments.ConvertAll(GenerateBytecode);
        if (BaseType.UnwrapParentheses(boundInvocation.Callee.Type) is not Types.FunctionType functionType)
            return NoOp(invocation);
        
        List<Instruction> argumentsBytecodeWithDefaults = [];
        var parameterTypes = functionType.ParameterTypes;
        var bytecodeIndex = 0;
        foreach (var (name, type) in parameterTypes)
        {
            var defaultValue = functionType.Defaults.GetValueOrDefault(name);
            var argumentBytecode = argumentsBytecode.ElementAtOrDefault(bytecodeIndex++);
            if (argumentBytecode != null)
            {
                argumentsBytecodeWithDefaults.AddRange(argumentBytecode);
                continue;
            }
                
            argumentsBytecodeWithDefaults.Add(new(invocation, OpCode.PUSH, defaultValue));
        }

        var argumentsOptimizer = new BytecodeOptimizer(argumentsBytecodeWithDefaults, diagnostics);
        var optimizedArgumentsBytecode = argumentsOptimizer.Optimize();
        var operand = (optimizedArgumentsBytecode.Count, functionType.ParameterTypes.Keys.ToList());
        return [
            ..GenerateBytecode(invocation.Callee),
            new(invocation, OpCode.CALL, operand),
            ..argumentsBytecodeWithDefaults
        ];
    }
    
    public List<Instruction> VisitReturnStatement(Return @return) =>
    [
        ..GenerateBytecode(@return.Expression),
        new(@return, OpCode.RETURN)
    ];
    
    public List<Instruction> VisitBreakStatement(Break @break) =>
    [
        new(@break, OpCode.BREAK, 69420)
    ];
    
    public List<Instruction> VisitContinueStatement(Continue @continue) =>
    [
        new(@continue, OpCode.CONTINUE, 69420)
    ];

    public List<Instruction> VisitExpressionStatement(ExpressionStatement expressionStatement) =>
        GenerateBytecode(expressionStatement.Expression);

    public List<Instruction> VisitNoOp(NoOp noOp) => NoOp(noOp);
    public List<Instruction> VisitNoOp(NoOpStatement noOp) => NoOp(noOp);
    public List<Instruction> VisitNoOp(NoOpType noOp) => NoOp(noOp);
    public List<Instruction> VisitNameOfExpression(NameOf nameOf) => NoOp(nameOf);

    public List<Instruction> VisitSingularTypeRef(SingularType singularType) => NoOp(singularType);
    public List<Instruction> VisitParenthesizedTypeRef(ParenthesizedType parenthesizedType) => NoOp(parenthesizedType);
    public List<Instruction> VisitUnionTypeRef(UnionType unionType) => NoOp(unionType);
    public List<Instruction> VisitIntersectionTypeRef(IntersectionType intersectionType) => NoOp(intersectionType);
    public List<Instruction> VisitFunctionTypeRef(FunctionType functionType) => NoOp(functionType);

    public List<Instruction> VisitAssignmentOpExpression(AssignmentOp assignmentOp) => VisitBinaryOpExpression(assignmentOp);
    public List<Instruction> VisitBinaryOpExpression(BinaryOp binaryOp)
    {
        if (binder.GetBoundNode(binaryOp) is not BoundBinaryOp boundBinaryOp)
            return NoOp(binaryOp);

        var leftInstructions = GenerateBytecode(binaryOp.Left);
        var rightInstructions = GenerateBytecode(binaryOp.Right);
        var combined = leftInstructions.Concat(rightInstructions);
        var boundOperatorType = boundBinaryOp.Operator.Type;

        if (boundOperatorType == BoundBinaryOperatorType.Assignment)
        {
            return binaryOp.Left is Name name
                ? [
                    ..PushName(name),
                    ..rightInstructions,
                    new Instruction(binaryOp, OpCode.STORE, true)
                ]
                : [
                    ..PushAssignmentTarget((AssignmentTarget)binaryOp.Left),
                    ..rightInstructions,
                    new Instruction(binaryOp, OpCode.STOREINDEX, true)
                ];
        }
        
        if (BoundBinaryOperator.OpCodeMap.TryGetValue(boundOperatorType, out var opCode))
        {
            return (!SyntaxFacts.BinaryCompoundAssignmentOperators.Contains(binaryOp.Operator.Kind)
                    ? combined.Append(new Instruction(binaryOp, opCode))
                    : (AssignmentTarget)binaryOp.Left is Name name
                        ? [
                            ..PushName(name),
                            ..leftInstructions,
                            ..rightInstructions,
                            new Instruction(binaryOp, opCode),
                            new Instruction(binaryOp, OpCode.STORE, true)
                        ]
                        : [
                            ..PushAssignmentTarget((AssignmentTarget)binaryOp.Left),
                            ..leftInstructions,
                            ..rightInstructions,
                            new Instruction(binaryOp, opCode),
                            new Instruction(binaryOp, OpCode.STOREINDEX, true)
                        ]
                ).ToList();
        }

        diagnostics.Error(DiagnosticCode.H008, $"Unsupported binary operator kind: {binaryOp.Operator.Kind}", binaryOp.Operator);
        return NoOp(binaryOp);
    }
    
    public List<Instruction> VisitPostfixOpExpression(PostfixOp postfixOp)
    {
        var value = GenerateBytecode(postfixOp.Operand);
        var bytecode = postfixOp.Operator.Kind switch
        {
            _ => value
        };

        return bytecode.ToList();
    }

    public List<Instruction> VisitUnaryOpExpression(UnaryOp unaryOp)
    {
        var value = GenerateBytecode(unaryOp.Operand);
        var bytecode = unaryOp.Operator.Kind switch
        {
            SyntaxKind.Bang => value.Append(new Instruction(unaryOp, OpCode.NOT)),
            SyntaxKind.Tilde => value.Append(new Instruction(unaryOp, OpCode.BNOT)),
            SyntaxKind.Minus => value.Append(new Instruction(unaryOp, OpCode.UNM)),
            SyntaxKind.PlusPlus => value.Append(new Instruction(unaryOp, OpCode.INC)),
            SyntaxKind.MinusMinus => value.Append(new Instruction(unaryOp, OpCode.DEC)),

            _ => null! // TODO: error
        };

        return bytecode.ToList();
    }

    public List<Instruction> VisitIdentifierNameExpression(IdentifierName identifierName) => [
        new(identifierName, OpCode.PUSH, identifierName.Token.Text),
        new(identifierName, OpCode.LOAD)
    ];

    public List<Instruction> VisitParenthesizedExpression(Parenthesized parenthesized) => GenerateBytecode(parenthesized.Expression);
    public List<Instruction> VisitLiteralExpression(Literal literal) =>
        [new(literal, literal.Token.Value != null ? OpCode.PUSH : OpCode.PUSHNONE, literal.Token.Value)];
    public List<Instruction> VisitObjectLiteralExpression(ObjectLiteral objectLiteral)
    {
        // store objects as dictionaries, for now (this may be permanent tbh)
        var objectValue = new Dictionary<List<Instruction>, List<Instruction>>(
            objectLiteral.Properties
                .ToList()
                .ConvertAll<KeyValuePair<List<Instruction>, List<Instruction>>>(property =>
                {
                    var keyExpression = property.Key;
                    if (keyExpression is IdentifierName identifier)
                        keyExpression = new Literal(TokenFactory.StringFromIdentifier(identifier.Token));

                    var key = GenerateBytecode(keyExpression);
                    var value = GenerateBytecode(property.Value);
                    return new(key, value);
                })
        );

        return [new Instruction(objectLiteral, OpCode.PUSHOBJECT, objectValue)];
    }

    private List<Instruction> PushAssignmentTarget(AssignmentTarget assignmentTarget)
    {
        switch (assignmentTarget)
        {
            case Name name:
                return PushName(name);
            case MemberAccess memberAccess:
                return
                [
                    ..GenerateBytecode(memberAccess.Expression),
                    ..PushName(memberAccess.Name)
                ];
            case ElementAccess elementAccess:
                return
                [
                    ..GenerateBytecode(elementAccess.Expression),
                    ..GenerateBytecode(elementAccess.IndexExpression)
                ];
            
            default:
                diagnostics.Error(DiagnosticCode.HDEV,
                    $"Unhandled assignment target type in PushAssignmentTarget: {assignmentTarget.GetType()}",
                    assignmentTarget);

                return NoOp(assignmentTarget);
        }
    }
    
    private static List<Instruction> PushName(Name name) => [new(name, OpCode.PUSH, name.ToString())];
    private static List<Instruction> NoOp(SyntaxNode node) => [new(node, OpCode.NOOP)];

    private List<Instruction> GenerateStatementsBytecode(List<Statement> statements) => statements.SelectMany(GenerateBytecode).ToList();
    private List<Instruction> GenerateBytecode(Expression expression) => expression.Accept(this);
    private List<Instruction> GenerateBytecode(Statement statement) => statement.Accept(this);
    private List<Instruction> GenerateBytecode(SyntaxNode node)
    {
        return node switch
        {
            Expression expression => GenerateBytecode(expression),
            Statement statement => GenerateBytecode(statement),
            _ => []
        };
    }
}