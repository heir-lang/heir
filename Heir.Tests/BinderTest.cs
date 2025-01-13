using Heir.BoundAST;
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
    public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
    {
        var boundTree = Bind(input);
        Assert.True(boundTree.Diagnostics.HasErrors);
        Assert.Contains(boundTree.Diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
    }

    [Theory]
    [InlineData("fn add(x: int, y = 1): int { return x + y; }")]
    [InlineData("fn add(x: int, y = 1): int -> x + y;")]
    [InlineData("fn add(x: int, y = 1) { return x + y; }")]
    [InlineData("fn add(x: int, y = 1) -> x + y;")]
    [InlineData("fn add(x: int, y: int = 1) { return x + y; }")]
    public void Binds_FunctionDeclarations_WithParameters(string input)
    {
        var tree = Bind(input);
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
        Assert.IsType<PrimitiveType>(xParameter.Type);
        Assert.IsType<PrimitiveType>(yParameter.Type);
        Assert.Null(xParameter.Initializer);
        Assert.IsType<BoundLiteral>(yParameter.Initializer);
        
        var xType = (PrimitiveType)xParameter.Type;
        var yType = (PrimitiveType)xParameter.Type;
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
        Assert.IsType<PrimitiveType>(binaryOpType.Types.First());
        Assert.IsType<PrimitiveType>(binaryOpType.Types.Last());

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
        Assert.IsType<PrimitiveType>(left.Type);
        Assert.IsType<PrimitiveType>(right.Type);
    }

    [Theory]
    [InlineData("fn abc: int { return 123; }")]
    [InlineData("fn abc: int -> 123;")]
    [InlineData("fn abc { return 123; }")]
    [InlineData("fn abc -> 123;")]
    public void Binds_FunctionDeclarations(string input)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundFunctionDeclaration>(statement);
        
        var functionDeclaration = (BoundFunctionDeclaration)statement;
        Assert.Empty(functionDeclaration.Parameters);
        Assert.Empty(functionDeclaration.Type.ParameterTypes);
        Assert.Equal("abc", functionDeclaration.Symbol.Name.Text);
        Assert.IsType<PrimitiveType>(functionDeclaration.Type.ReturnType);
        
        var returnType = (PrimitiveType)functionDeclaration.Type.ReturnType;
        Assert.Equal(PrimitiveTypeKind.Int, returnType.PrimitiveKind);
        Assert.Single(functionDeclaration.Body.Statements);
        Assert.IsType<BoundReturn>(functionDeclaration.Body.Statements.First());
        
        var returnStatement = (BoundReturn)functionDeclaration.Body.Statements.First();
        Assert.IsType<BoundLiteral>(returnStatement.Expression);
        
        var literal = (BoundLiteral)returnStatement.Expression;
        Assert.Equal(123L, literal.Token.Value);
    }
    
    [Theory]
    [InlineData("fn abc(x = 0, y = 0) {} abc();", 0)]
    [InlineData("fn abc(x = 0, y = 0) {} abc(69);", 1)]
    [InlineData("fn abc(x = 0, y = 0) {} abc(69, 420);", 2)]
    public void Binds_Invocation(string input, int expectedArgumentCount)
    {
        var tree = Bind(input);
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
        Assert.Equal(2, functionType.ParameterTypes.Count);
        Assert.IsType<PrimitiveType>(functionType.ReturnType);
        
        var returnType = (PrimitiveType)functionType.ReturnType;
        Assert.Equal(PrimitiveTypeKind.None, returnType.PrimitiveKind);
        
        Assert.Equal(expectedArgumentCount, invocation.Arguments.Count);
        foreach (var argument in invocation.Arguments)
            Assert.IsType<BoundLiteral>(argument);
    }

    [Fact]
    public void Binds_ReturnStatements()
    {
        var boundTree = Bind("return 123;");
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundReturn>(statement);
        
        var returnStatement = (BoundReturn)statement;
        Assert.IsType<PrimitiveType>(returnStatement.Type);
        Assert.IsType<BoundLiteral>(returnStatement.Expression);
        
        var primitiveType = (PrimitiveType)returnStatement.Type;
        Assert.Equal(PrimitiveTypeKind.Int, primitiveType.PrimitiveKind);
    }

    [Fact]
    public void Binds_Identifiers()
    {
        var boundTree = Bind("let x: string; x;");
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
        var boundTree = Bind("let x: string;");
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
        var boundTree = Bind("let mut x = 1;");
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundVariableDeclaration>(statement);

        var declaration = (BoundVariableDeclaration)statement;
        Assert.True(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.IsType<BoundLiteral>(declaration.Initializer);

        Assert.NotNull(declaration.Type);
        Assert.IsType<PrimitiveType>(declaration.Type);

        var type = (PrimitiveType)declaration.Type;
        Assert.Equal("int", type.Name);
    }

    [Theory]
    [InlineData("{ a: true }", "a")]
    [InlineData("{ [\"a\"]: true }", "a")]
    [InlineData("{ [1]: true }", 1L)]
    public void Parses_ObjectLiterals(string input, object? keyValue)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundObjectLiteral>(node);

        var objectLiteral = (BoundObjectLiteral)node;
        Assert.Single(objectLiteral.Properties);

        var keyType = objectLiteral.Properties.Keys.First();
        var value = objectLiteral.Properties.Values.First();
        Assert.IsType<LiteralType>(keyType);
        Assert.IsType<PrimitiveType>(value.Type);
        Assert.IsType<BoundLiteral>(value);

        var keyLiteralType = (LiteralType)keyType;
        var valueLiteralType = (PrimitiveType)value.Type;
        Assert.Equal(keyValue, keyLiteralType.Value);
        Assert.Equal(PrimitiveTypeKind.Bool, valueLiteralType.PrimitiveKind);
    }

    [Theory]
    [InlineData("\"abc\"", PrimitiveTypeKind.String)]
    [InlineData("'a'", PrimitiveTypeKind.Char)]
    [InlineData("123", PrimitiveTypeKind.Int)]
    [InlineData("69", PrimitiveTypeKind.Int)]
    [InlineData("0b1101", PrimitiveTypeKind.Int)]
    [InlineData("0o420", PrimitiveTypeKind.Int)]
    [InlineData("0x03E", PrimitiveTypeKind.Int)]
    [InlineData("123.456", PrimitiveTypeKind.Float)]
    [InlineData("69.420", PrimitiveTypeKind.Float)]
    [InlineData("true", PrimitiveTypeKind.Bool)]
    [InlineData("false", PrimitiveTypeKind.Bool)]
    [InlineData("none", PrimitiveTypeKind.None)]
    public void Binds_Literals(string input, PrimitiveTypeKind primitiveTypeKind)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundLiteral>(node);

        var literal = (BoundLiteral)node;
        Assert.Equal(input, literal.Token.Text);
        Assert.Equal(primitiveTypeKind, literal.Type.PrimitiveKind);
    }

    [Theory]
    [InlineData("true && false", PrimitiveTypeKind.Bool)]
    [InlineData("8 // 3", PrimitiveTypeKind.Int)]
    [InlineData("\"a\" + \"b\"", PrimitiveTypeKind.String)]
    [InlineData("'a' + 'b'", PrimitiveTypeKind.Char)]
    public void Binds_PrimitiveBinaryOperations(string input, PrimitiveTypeKind returnTypeKind)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundBinaryOp>(node);

        var binaryOp = (BoundBinaryOp)node;
        Assert.IsType<PrimitiveType>(binaryOp.Left.Type);
        Assert.IsType<PrimitiveType>(binaryOp.Right.Type);
        Assert.IsType<PrimitiveType>(binaryOp.Type);

        var returnType = (PrimitiveType)binaryOp.Type;
        Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
    }

    [Theory]
    [InlineData("7 / 4", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)]
    [InlineData("\"a\" + 'b'", PrimitiveTypeKind.String, PrimitiveTypeKind.Char)]
    public void Binds_UnionBinaryOperations(string input, PrimitiveTypeKind typeAKind, PrimitiveTypeKind typeBKind)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundBinaryOp>(node);

        var binaryOp = (BoundBinaryOp)node;
        Assert.IsType<PrimitiveType>(binaryOp.Left.Type);
        Assert.IsType<PrimitiveType>(binaryOp.Right.Type);
        Assert.IsType<UnionType>(binaryOp.Type);

        var returnType = (UnionType)binaryOp.Type;
        var typeA = returnType.Types.First();
        var typeB = returnType.Types.Last();
        Assert.IsType<PrimitiveType>(typeA);
        Assert.IsType<PrimitiveType>(typeB);
        Assert.Equal(typeAKind, ((PrimitiveType)typeA).PrimitiveKind);
        Assert.Equal(typeBKind, ((PrimitiveType)typeB).PrimitiveKind);
        Assert.Equal(TypeKind.Union, returnType.Kind);
    }

    [Theory]
    [InlineData("!false", PrimitiveTypeKind.Bool)]
    [InlineData("~6", PrimitiveTypeKind.Int)]
    public void Binds_PrimitiveUnaryOperations(string input, PrimitiveTypeKind returnTypeKind)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundUnaryOp>(node);

        var unaryOp = (BoundUnaryOp)node;
        Assert.IsType<PrimitiveType>(unaryOp.Operand.Type);
        Assert.IsType<PrimitiveType>(unaryOp.Type);

        var returnType = (PrimitiveType)unaryOp.Type;
        Assert.Equal(returnTypeKind, returnType.PrimitiveKind);
    }

    [Theory]
    [InlineData("-69.420", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)]
    [InlineData("++6", PrimitiveTypeKind.Int, PrimitiveTypeKind.Float)] // invalid but only for testing purposes so idc
    public void Binds_UnionUnaryOperations(string input, PrimitiveTypeKind typeAKind, PrimitiveTypeKind typeBKind)
    {
        var boundTree = Bind(input);
        var statement = boundTree.Statements.First();
        Assert.IsType<BoundExpressionStatement>(statement);

        var node = ((BoundExpressionStatement)statement).Expression;
        Assert.IsType<BoundUnaryOp>(node);

        var unaryOp = (BoundUnaryOp)node;
        Assert.IsType<PrimitiveType>(unaryOp.Operand.Type);
        Assert.IsType<UnionType>(unaryOp.Type);

        var returnType = (UnionType)unaryOp.Type;
        var typeA = returnType.Types.First();
        var typeB = returnType.Types.Last();
        Assert.IsType<PrimitiveType>(typeA);
        Assert.IsType<PrimitiveType>(typeB);
        Assert.Equal(typeAKind, ((PrimitiveType)typeA).PrimitiveKind);
        Assert.Equal(typeBKind, ((PrimitiveType)typeB).PrimitiveKind);
        Assert.Equal(TypeKind.Union, returnType.Kind);
    }
}
