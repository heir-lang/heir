﻿using Heir.Binding;
using Heir.BoundAST;
using Heir.Diagnostics;
using Heir.Syntax;
using Heir.Types;
using static Heir.Tests.Common;

namespace Heir.Tests;

public class BinderTest
{
    [Theory]
    [InlineData("\"a\" + 1", DiagnosticCode.H007)]
    [InlineData("true * false", DiagnosticCode.H007)]
    [InlineData("let x = 1; x = 2;", DiagnosticCode.H006C)]
    [InlineData("let a: A;", DiagnosticCode.H005)]
    [InlineData("a;", DiagnosticCode.H005)]
    public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        Assert.True(boundTree.Diagnostics.HasErrors);
        Assert.Contains(boundTree.Diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
    }
    
    [Theory]
    [InlineData("interface A; let a: A;")]
    [InlineData("interface Foo { bar: string; } let foo: Foo = { bar: \"baz\" };")]
    [InlineData("let a: int; a;")]
    public void DoesNotThrowWith(string input)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        Assert.Empty(boundTree.Diagnostics);
    }

    [Fact]
    public void Binds_InterfaceDeclarations()
    {
        const string input = """
                             interface Abc {
                                a: int;
                                mut b: string;
                             }
                             """;
        
        var binder = Bind(input);
        var interfaceDeclaration = (AST.InterfaceDeclaration)binder.SyntaxTree.Statements.First();
        var symbol = binder.FindTypeSymbol(interfaceDeclaration.Identifier);
        Assert.NotNull(symbol);
        Assert.False(symbol.IsIntrinsic);
        Assert.IsType<InterfaceType>(symbol.Type);
        
        var interfaceType = (InterfaceType)symbol.Type;
        Assert.Equal("Abc", interfaceType.Name);
        Assert.Empty(interfaceType.IndexSignatures);
        Assert.Equal(2, interfaceType.Members.Count);

        var aField = interfaceType.Members.First();
        var bField = interfaceType.Members.Last();
        Assert.Equal("a", aField.Key.Value);
        Assert.False(aField.Value.IsMutable);
        Assert.IsType<PrimitiveType>(aField.Value.Type);
        Assert.Equal("b", bField.Key.Value);
        Assert.True(bField.Value.IsMutable);
        Assert.IsType<PrimitiveType>(bField.Value.Type);
        
        var aType = (PrimitiveType)aField.Value.Type;
        var bType = (PrimitiveType)bField.Value.Type;
        Assert.Equal(PrimitiveTypeKind.Int, aType.PrimitiveKind);
        Assert.Equal(PrimitiveTypeKind.String, bType.PrimitiveKind);
    }
    
    [Fact]
    public void Binds_MemberAccess()
    {
        const string input = """
                             let foo = { bar: "baz" };

                             foo.bar;
                             """;
        
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);
        
        var expressionStatement = (BoundExpressionStatement)statement;
        Assert.IsType<BoundMemberAccess>(expressionStatement.Expression);
        
        var memberAccess = (BoundMemberAccess)expressionStatement.Expression;
        Assert.IsType<BoundIdentifierName>(memberAccess.Expression);
        Assert.IsType<InterfaceType>(memberAccess.Expression.Type);
        
        var interfaceType = (InterfaceType)memberAccess.Expression.Type;
        Assert.Single(interfaceType.Members);
        Assert.IsType<LiteralType>(memberAccess.Type);
        
        var type = (LiteralType)memberAccess.Type;
        Assert.Equal("baz", type.Value);
    }

    [Fact]
    public void Binds_ElementAccessForObjects()
    {
        const string input = """
                             let foo = { bar: "baz" };
                             
                             foo["bar"];
                             """;
        
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);
        
        var expressionStatement = (BoundExpressionStatement)statement;
        Assert.IsType<BoundElementAccess>(expressionStatement.Expression);
        
        var elementAccess = (BoundElementAccess)expressionStatement.Expression;
        Assert.IsType<BoundIdentifierName>(elementAccess.Expression);
        Assert.IsType<InterfaceType>(elementAccess.Expression.Type);
        
        var interfaceType = (InterfaceType)elementAccess.Expression.Type;
        Assert.Single(interfaceType.Members);
        
        Assert.IsType<BoundLiteral>(elementAccess.IndexExpression);
        Assert.IsType<LiteralType>(elementAccess.IndexExpression.Type);
        Assert.IsType<LiteralType>(elementAccess.Type);
        
        var type = (LiteralType)elementAccess.Type;
        Assert.Equal("baz", type.Value);
    }
    
    [Fact]
    public void Binds_ElementAccessForArrays()
    {
        const string input = """
                             let foo = [1, 2, 3];

                             foo[0];
                             """;
        
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);
        
        var expressionStatement = (BoundExpressionStatement)statement;
        Assert.IsType<BoundElementAccess>(expressionStatement.Expression);
        
        var elementAccess = (BoundElementAccess)expressionStatement.Expression;
        Assert.IsType<BoundIdentifierName>(elementAccess.Expression);
        Assert.IsType<ArrayType>(elementAccess.Expression.Type);
        
        var arrayType = (ArrayType)elementAccess.Expression.Type;
        Assert.IsType<PrimitiveType>(arrayType.ElementType);
        
        Assert.IsType<BoundLiteral>(elementAccess.IndexExpression);
        Assert.IsType<LiteralType>(elementAccess.IndexExpression.Type);
        Assert.IsType<PrimitiveType>(elementAccess.Type);
        
        var type = (PrimitiveType)elementAccess.Type;
        Assert.Equal(PrimitiveTypeKind.Int, type.PrimitiveKind);
    }

    [Fact]
    public void Binds_WhileStatements()
    {
        const string input = """
                             while i < 10
                                ++i;
                             """;
        
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.First();
        Assert.IsType<BoundWhile>(statement);
        
        var whileStatement = (BoundWhile)statement;
        Assert.IsType<BoundBinaryOp>(whileStatement.Condition);
        Assert.IsType<BoundExpressionStatement>(whileStatement.Body);
        Assert.Null(whileStatement.Type);
        
        var conditionBinaryOp = (BoundBinaryOp)whileStatement.Condition;
        Assert.IsType<BoundIdentifierName>(conditionBinaryOp.Left);
        Assert.IsType<BoundLiteral>(conditionBinaryOp.Right);
        
        var body = (BoundExpressionStatement)whileStatement.Body;
        Assert.IsType<BoundUnaryOp>(body.Expression);
    }

    [Fact]
    public void Binds_IfStatements()
    {
        const string input = """
                             if a
                                69 + 420;
                             else if b
                                420 - 69;
                             else
                                69;
                             """;
        
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.First();
        Assert.IsType<BoundIf>(statement);
        
        var ifStatement = (BoundIf)statement;
        Assert.IsType<BoundIdentifierName>(ifStatement.Condition);
        Assert.IsType<BoundExpressionStatement>(ifStatement.Body);
        Assert.IsType<BoundIf>(ifStatement.ElseBranch);
        Assert.Null(ifStatement.Type);
        
        var mainCondition = (BoundIdentifierName)ifStatement.Condition;
        Assert.Equal("a", mainCondition.Symbol.Name.Text);
        
        var mainBody = (BoundExpressionStatement)ifStatement.Body;
        Assert.IsType<BoundBinaryOp>(mainBody.Expression);
        Assert.IsType<UnionType>(mainBody.Expression.Type);
        
        var elseIf = (BoundIf)ifStatement.ElseBranch;
        Assert.IsType<BoundIdentifierName>(elseIf.Condition);
        Assert.IsType<BoundExpressionStatement>(elseIf.Body);
        Assert.IsType<BoundExpressionStatement>(elseIf.ElseBranch);
        
        var elseIfCondition = (BoundIdentifierName)elseIf.Condition;
        Assert.Equal("b", elseIfCondition.Symbol.Name.Text);
        
        var elseIfBody = (BoundExpressionStatement)elseIf.Body;
        Assert.IsType<BoundBinaryOp>(elseIfBody.Expression);
        Assert.IsType<UnionType>(elseIfBody.Expression.Type);
        
        var elseBranch = (BoundExpressionStatement)elseIf.ElseBranch;
        Assert.IsType<BoundLiteral>(elseBranch.Expression);
        Assert.IsAssignableFrom<PrimitiveType>(elseBranch.Expression.Type);
    }

    [Theory]
    [InlineData("fn add(x: int, y = 1): int { return x + y; }")]
    [InlineData("fn add(x: int, y = 1): int -> x + y;")]
    [InlineData("fn add(x: int, y = 1) { return x + y; }")]
    [InlineData("fn add(x: int, y = 1) -> x + y;")]
    [InlineData("fn add(x: int, y: int = 1) { return x + y; }")]
    public void Binds_FunctionDeclarations_WithParameters(string input)
    {
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.First();
        Assert.IsType<BoundFunctionDeclaration>(statement);
        
        var functionDeclaration = (BoundFunctionDeclaration)statement;
        Assert.Equal(SyntaxKind.FnKeyword, functionDeclaration.Keyword.Kind);
        Assert.Equal("add", functionDeclaration.Symbol.Name.Text);
        Assert.Equal(2, functionDeclaration.Parameters.Count);
        
        var xParameter = functionDeclaration.Parameters.First();
        var yParameter = functionDeclaration.Parameters.Last();
        Assert.Equal("x", xParameter.Symbol.Name.Text);
        Assert.Equal("y", yParameter.Symbol.Name.Text);
        Assert.IsAssignableFrom<PrimitiveType>(xParameter.Type);
        Assert.IsAssignableFrom<PrimitiveType>(yParameter.Type);
        Assert.Null(xParameter.Initializer);
        Assert.IsType<BoundLiteral>(yParameter.Initializer);
        
        var xType = (PrimitiveType)xParameter.Type;
        var yType = (PrimitiveType)yParameter.Type;
        Assert.Equal(PrimitiveTypeKind.Int, xType.PrimitiveKind);
        Assert.Equal(PrimitiveTypeKind.Int, yType.PrimitiveKind);

        Assert.Single(functionDeclaration.Body.Statements);
        Assert.IsType<BoundReturn>(functionDeclaration.Body.Statements.First());
        
        var returnStatement = (BoundReturn)functionDeclaration.Body.Statements.First();
        Assert.IsType<BoundBinaryOp>(returnStatement.Expression);
        
        var binaryOp = (BoundBinaryOp)returnStatement.Expression;
        Assert.IsType<UnionType>(binaryOp.Type);

        var binaryOpType = (UnionType)binaryOp.Type;
        Assert.Equal(2, binaryOpType.Types.Count);
        Assert.IsAssignableFrom<PrimitiveType>(binaryOpType.Types.First());
        Assert.IsAssignableFrom<PrimitiveType>(binaryOpType.Types.Last());

        var binaryOpType1 = (PrimitiveType)binaryOpType.Types.First();
        var binaryOpType2 = (PrimitiveType)binaryOpType.Types.Last();
        Assert.Equal(PrimitiveTypeKind.Int, binaryOpType1.PrimitiveKind);
        Assert.Equal(PrimitiveTypeKind.Float, binaryOpType2.PrimitiveKind);
        Assert.Equal(BoundBinaryOperatorType.Addition, binaryOp.Operator.Type);
        Assert.IsType<BoundIdentifierName>(binaryOp.Left);
        Assert.IsType<BoundIdentifierName>(binaryOp.Right);

        var left = (BoundIdentifierName)binaryOp.Left;
        var right = (BoundIdentifierName)binaryOp.Right;
        Assert.Equal("x", left.Symbol.Name.Text);
        Assert.Equal("y", right.Symbol.Name.Text);
        Assert.IsAssignableFrom<PrimitiveType>(left.Type);
        Assert.IsAssignableFrom<PrimitiveType>(right.Type);
    }

    [Theory]
    [InlineData("fn abc: int { return 123; }")]
    [InlineData("fn abc: int -> 123;")]
    [InlineData("fn abc { return 123; }")]
    [InlineData("fn abc -> 123;")]
    public void Binds_FunctionDeclarations(string input)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundFunctionDeclaration>(statement);
        
        var functionDeclaration = (BoundFunctionDeclaration)statement;
        Assert.Empty(functionDeclaration.Parameters);
        Assert.Empty(functionDeclaration.Type.Parameters);
        Assert.Equal("abc", functionDeclaration.Symbol.Name.Text);
        Assert.IsAssignableFrom<PrimitiveType>(functionDeclaration.Type.ReturnType);
        
        var returnType = (PrimitiveType)functionDeclaration.Type.ReturnType;
        Assert.Equal(PrimitiveTypeKind.Int, returnType.PrimitiveKind);
        Assert.Single(functionDeclaration.Body.Statements);
        Assert.IsType<BoundReturn>(functionDeclaration.Body.Statements.First());
        
        var returnStatement = (BoundReturn)functionDeclaration.Body.Statements.First();
        Assert.IsType<BoundLiteral>(returnStatement.Expression);
        
        var literal = (BoundLiteral)returnStatement.Expression;
        Assert.Equal(123, literal.Token.Value);
    }
    
    [Theory]
    [InlineData("fn abc(x = 0, y = 0) {} abc();", 0)]
    [InlineData("fn abc(x = 0, y = 0) {} abc(69);", 1)]
    [InlineData("fn abc(x = 0, y = 0) {} abc(69, 420);", 2)]
    public void Binds_Invocation(string input, int expectedArgumentCount)
    {
        var tree = Bind(input).GetBoundSyntaxTree();
        var statement = tree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);
        
        var expressionStatement = (BoundExpressionStatement)statement;
        Assert.IsType<BoundInvocation>(expressionStatement.Expression);
        
        var invocation = (BoundInvocation)expressionStatement.Expression;
        Assert.IsType<BoundIdentifierName>(invocation.Callee);
        
        var calleeName = (BoundIdentifierName)invocation.Callee;
        Assert.Equal("abc", calleeName.Symbol.Name.Text);
        Assert.IsType<FunctionType>(calleeName.Type);
        
        var functionType = (FunctionType)calleeName.Type;
        Assert.Equal(2, functionType.Parameters.Count);
        Assert.IsAssignableFrom<PrimitiveType>(functionType.ReturnType);
        
        var returnType = (PrimitiveType)functionType.ReturnType;
        Assert.Equal(PrimitiveTypeKind.None, returnType.PrimitiveKind);
        
        Assert.Equal(expectedArgumentCount, invocation.Arguments.Count);
        foreach (var argument in invocation.Arguments)
            Assert.IsType<BoundLiteral>(argument);
    }

    [Fact]
    public void Binds_ReturnStatements()
    {
        var boundTree = Bind("return 123;").GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundReturn>(statement);
        
        var returnStatement = (BoundReturn)statement;
        Assert.IsType<LiteralType>(returnStatement.Type);
        Assert.IsType<BoundLiteral>(returnStatement.Expression);
        
        var literalType = (LiteralType)returnStatement.Type;
        Assert.Equal(123, literalType.Value);
    }

    [Fact]
    public void Binds_Identifiers()
    {
        var boundTree = Bind("let x: string; x;").GetBoundSyntaxTree();
        var statement = boundTree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundIdentifierName>(node);

        var identifier = (BoundIdentifierName)node;
        Assert.Equal("x", identifier.Symbol.Name.Text);
        Assert.IsType<PrimitiveType>(identifier.Type);

        var type = (PrimitiveType)identifier.Type;
        Assert.Equal("string", type.Name);
    }

    [Fact]
    public void Binds_VariableDeclarations()
    {
        var boundTree = Bind("let x: string;").GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundVariableDeclaration>(statement);

        var declaration = (BoundVariableDeclaration)statement;
        Assert.False(declaration.IsMutable);
        Assert.Null(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<PrimitiveType>(declaration.Type);

        var type = (PrimitiveType)declaration.Type;
        Assert.Equal("string", type.Name);
    }

    [Fact]
    public void Infers_VariableDeclarationTypes()
    {
        var boundTree = Bind("let mut x = 1;").GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundVariableDeclaration>(statement);

        var declaration = (BoundVariableDeclaration)statement;
        Assert.True(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.IsType<BoundLiteral>(declaration.Initializer);

        Assert.NotNull(declaration.Type);
        Assert.IsType<PrimitiveType>(declaration.Type);

        var primitiveType = (PrimitiveType)declaration.Type;
        Assert.Equal(PrimitiveTypeKind.Int, primitiveType.PrimitiveKind);
    }
    
    [Fact]
    public void Binds_EnumDeclarations()
    {
        var binder = Bind("enum Abc { A, B, C }");
        var boundTree = binder.GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundEnumDeclaration>(statement);

        var declaration = (BoundEnumDeclaration)statement;
        Assert.False(declaration.IsInline);
        Assert.Equal(3, declaration.Members.Count);
        Assert.IsType<InterfaceType>(declaration.Type);
        
        var interfaceType = (InterfaceType)declaration.Type;
        Assert.Equal(3, interfaceType.Members.Count);

        Assert.IsType<UnionType>(declaration.TypeSymbol.Type);
    }

    [Theory]
    [InlineData("{ a: true }", "a")]
    [InlineData("{ [\"a\"]: true }", "a")]
    [InlineData("{ [1]: true }", 1)]
    public void Binds_ObjectLiterals(string input, object? keyValue)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundObjectLiteral>(node);

        var objectLiteral = (BoundObjectLiteral)node;
        Assert.Single(objectLiteral.Properties);

        var keyType = objectLiteral.Properties.Keys.First();
        var value = objectLiteral.Properties.Values.First();
        Assert.IsType<LiteralType>(keyType);
        Assert.IsType<LiteralType>(value.Type);
        Assert.IsType<BoundLiteral>(value);

        var keyLiteralType = (LiteralType)keyType;
        var valueLiteralType = (LiteralType)value.Type;
        Assert.Equal(keyValue, keyLiteralType.Value);
        Assert.Equal(true, valueLiteralType.Value);
    }
    
    [Fact]
    public void Binds_EmptyArrayLiterals()
    {
        var tree = Bind("[]").GetBoundSyntaxTree();
        var statement = tree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);
        
        var expressionStatement = (BoundExpressionStatement)statement;
        Assert.IsType<BoundArrayLiteral>(expressionStatement.Expression);
        
        var arrayLiteral = (BoundArrayLiteral)expressionStatement.Expression;
        Assert.IsType<AnyType>(arrayLiteral.Type.ElementType);
        Assert.Equal(SyntaxKind.ArrayLiteral, arrayLiteral.Token.Kind);
        Assert.Empty(arrayLiteral.Elements);
    }
    
    [Fact]
    public void Binds_ArrayLiterals()
    {
        var tree = Bind("[1, 'a']").GetBoundSyntaxTree();
        var statement = tree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);
        
        var expressionStatement = (BoundExpressionStatement)statement;
        Assert.IsType<BoundArrayLiteral>(expressionStatement.Expression);
        
        var arrayLiteral = (BoundArrayLiteral)expressionStatement.Expression;
        Assert.Equal(SyntaxKind.ArrayLiteral, arrayLiteral.Token.Kind);
        Assert.Equal(2, arrayLiteral.Elements.Count);
        Assert.IsType<UnionType>(arrayLiteral.Type.ElementType);
        
        var elementType = (UnionType)arrayLiteral.Type.ElementType;
        Assert.Equal(2, elementType.Types.Count);
        
        var typeOne = elementType.Types.First();
        var typeTwo = elementType.Types.Last();
        Assert.IsType<PrimitiveType>(typeOne);
        Assert.IsType<PrimitiveType>(typeTwo);
        
        var elementOne = arrayLiteral.Elements.First();
        var elementTwo = arrayLiteral.Elements.Last();
        Assert.IsType<BoundLiteral>(elementOne);
        Assert.IsType<BoundLiteral>(elementTwo);
        
        var literalOne = (BoundLiteral)elementOne;
        var literalTwo = (BoundLiteral)elementTwo;
        Assert.Equal(SyntaxKind.IntLiteral, literalOne.Token.Kind);
        Assert.Equal(1, literalOne.Token.Value);
        Assert.Equal(SyntaxKind.CharLiteral, literalTwo.Token.Kind);
        Assert.Equal('a', literalTwo.Token.Value);
    }

    [Theory]
    [InlineData("\"abc\"")]
    [InlineData("'a'")]
    [InlineData("123")]
    [InlineData("69")]
    [InlineData("0b1101")]
    [InlineData("0o420")]
    [InlineData("0x03E")]
    [InlineData("123.456")]
    [InlineData("69.420")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("none")]
    public void Binds_Literals(string input)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundLiteral>(node);

        var literal = (BoundLiteral)node;
        Assert.Equal(input, literal.Token.Text);
        Assert.Equal(literal.Token.Value, literal.Type.Value);
    }

    [Theory]
    [InlineData("true && false", PrimitiveTypeKind.Bool)]
    [InlineData("8 // 3", PrimitiveTypeKind.Int)]
    [InlineData("\"a\" + \"b\"", PrimitiveTypeKind.String)]
    [InlineData("'a' + 'b'", PrimitiveTypeKind.Char)]
    public void Binds_PrimitiveBinaryOperations(string input, PrimitiveTypeKind returnTypeKind)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundBinaryOp>(node);

        var binaryOp = (BoundBinaryOp)node;
        Assert.IsAssignableFrom<PrimitiveType>(binaryOp.Left.Type);
        Assert.IsAssignableFrom<PrimitiveType>(binaryOp.Right.Type);
        Assert.IsAssignableFrom<PrimitiveType>(binaryOp.Type);

        var returnType = (PrimitiveType)binaryOp.Type;
        Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
    }

    [Theory]
    [InlineData("7 / 4", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)]
    [InlineData("\"a\" + 'b'", PrimitiveTypeKind.String, PrimitiveTypeKind.Char)]
    public void Binds_UnionBinaryOperations(string input, PrimitiveTypeKind typeAKind, PrimitiveTypeKind typeBKind)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundBinaryOp>(node);

        var binaryOp = (BoundBinaryOp)node;
        Assert.IsAssignableFrom<PrimitiveType>(binaryOp.Left.Type);
        Assert.IsAssignableFrom<PrimitiveType>(binaryOp.Right.Type);
        Assert.IsType<UnionType>(binaryOp.Type);

        var returnType = (UnionType)binaryOp.Type;
        var typeA = returnType.Types.First();
        var typeB = returnType.Types.Last();
        Assert.IsAssignableFrom<PrimitiveType>(typeA);
        Assert.IsAssignableFrom<PrimitiveType>(typeB);
        Assert.Equal(typeAKind, ((PrimitiveType)typeA).PrimitiveKind);
        Assert.Equal(typeBKind, ((PrimitiveType)typeB).PrimitiveKind);
        Assert.Equal(TypeKind.Union, returnType.Kind);
    }
    
    [Fact]
    public void Binds_NullForgiving()
    {
        var boundTree = Bind("let x: int? = 1; x!;").GetBoundSyntaxTree();
        var statement = boundTree.Statements.Last();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundPostfixOp>(node);

        var postfixOp = (BoundPostfixOp)node;
        Assert.IsType<UnionType>(postfixOp.Operand.Type);
        Assert.IsAssignableFrom<PrimitiveType>(postfixOp.Type);

        var returnType = (PrimitiveType)postfixOp.Type;
        Assert.Equal(PrimitiveTypeKind.Int, returnType.PrimitiveKind);
    }

    [Theory]
    [InlineData("!false", PrimitiveTypeKind.Bool)]
    [InlineData("~6", PrimitiveTypeKind.Int)]
    public void Binds_PrimitiveUnaryOperations(string input, PrimitiveTypeKind returnTypeKind)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundUnaryOp>(node);

        var unaryOp = (BoundUnaryOp)node;
        Assert.IsAssignableFrom<PrimitiveType>(unaryOp.Operand.Type);
        Assert.IsAssignableFrom<PrimitiveType>(unaryOp.Type);

        var returnType = (PrimitiveType)unaryOp.Type;
        Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
    }

    [Theory]
    [InlineData("-69.420", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)]
    [InlineData("++6", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)] // invalid but only for testing purposes so idc
    public void Binds_UnionUnaryOperations(string input, PrimitiveTypeKind typeAKind, PrimitiveTypeKind typeBKind)
    {
        var boundTree = Bind(input).GetBoundSyntaxTree();
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundUnaryOp>(node);

        var unaryOp = (BoundUnaryOp)node;
        Assert.IsAssignableFrom<PrimitiveType>(unaryOp.Operand.Type);
        Assert.IsType<UnionType>(unaryOp.Type);

        var returnType = (UnionType)unaryOp.Type;
        var typeA = returnType.Types.First();
        var typeB = returnType.Types.Last();
        Assert.IsAssignableFrom<PrimitiveType>(typeA);
        Assert.IsAssignableFrom<PrimitiveType>(typeB);
        Assert.Equal(typeAKind, ((PrimitiveType)typeA).PrimitiveKind);
        Assert.Equal(typeBKind, ((PrimitiveType)typeB).PrimitiveKind);
        Assert.Equal(TypeKind.Union, returnType.Kind);
    }
}
