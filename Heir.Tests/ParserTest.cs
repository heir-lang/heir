using Heir.AST;
using Heir.Diagnostics;
using Heir.Syntax;
using static Heir.Tests.Common;

namespace Heir.Tests;

public class ParserTest
{
    [Theory]
    [InlineData("let inline x = none", DiagnosticCode.H022)]
    [InlineData("let inline x", DiagnosticCode.H022)]
    [InlineData("let inline mut x = 1", DiagnosticCode.H021)]
    [InlineData("let mut inline x = 1", DiagnosticCode.H021)]
    [InlineData("fn abc(x = x) {}", DiagnosticCode.H016)]
    [InlineData("fn abc(x) {}", DiagnosticCode.H012)]
    [InlineData("let x", DiagnosticCode.H012)]
    [InlineData("1 = 2", DiagnosticCode.H006B)]
    [InlineData("++1", DiagnosticCode.H006)]
    [InlineData("--3", DiagnosticCode.H006)]
    [InlineData("()", DiagnosticCode.H004D)]
    [InlineData("fn", DiagnosticCode.H004C)]
    [InlineData("let", DiagnosticCode.H004C)]
    [InlineData("fn abc(", DiagnosticCode.H004)]
    [InlineData("(1", DiagnosticCode.H004)]
    [InlineData("]", DiagnosticCode.H001B)]
    public void ThrowsWith(string input, DiagnosticCode expectedCode)
    {
        var tree = Parse(input);
        Assert.True(tree.Diagnostics.HasErrors);
        Assert.Contains(tree.Diagnostics, diagnostic => diagnostic.Code == expectedCode);
    }

    [Theory]
    [InlineData("let x: (int) = 1", DiagnosticCode.H014)]
    [InlineData("let x: (int | float) = 1", DiagnosticCode.H014)]
    public void WarnsWith(string input, DiagnosticCode expectedCode)
    {
        var tree = Parse(input);
        Assert.True(tree.Diagnostics.HasWarnings);
        Assert.False(tree.Diagnostics.HasErrors);
        Assert.Contains(tree.Diagnostics, diagnostic => diagnostic.Code == expectedCode);
    }

    [Theory]
    [InlineData("++a")]
    [InlineData("--b")]
    public void DoesNotThrowWith(string input)
    {
        var tree = Parse(input);
        Assert.Empty(tree.Diagnostics);
    }
    
    [Fact]
    public void Parses_InlineVariables()
    {
        var tree = Parse("let inline x = 1; x;");
        var statement = tree.Statements.Last();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<Literal>(expressionStatement.Expression);
        
        var literal = (Literal)expressionStatement.Expression;
        Assert.Equal(SyntaxKind.IntLiteral, literal.Token.Kind);
        Assert.Equal(1, literal.Token.Value);
    }
    
    [Theory]
    [InlineData("interface Abc; nameof(Abc);", "Abc")]
    [InlineData("let x = 1; nameof(x);", "x")]
    public void Parses_NameOf(string input, string expectedName)
    {
        var tree = Parse(input);
        var statement = tree.Statements.Last();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<Literal>(expressionStatement.Expression);
        
        var literal = (Literal)expressionStatement.Expression;
        Assert.Equal(SyntaxKind.StringLiteral, literal.Token.Kind);
        Assert.Equal(expectedName, literal.Token.Value);
    }

    [Fact]
    public void Parses_EmptyInterfaces()
    {
        var tree = Parse("interface A;");
        var statement = tree.Statements.First();
        Assert.IsType<InterfaceDeclaration>(statement);
        
        var interfaceDeclaration = (InterfaceDeclaration)statement;
        Assert.Equal(SyntaxKind.InterfaceKeyword, interfaceDeclaration.Keyword.Kind);
        Assert.Equal("A", interfaceDeclaration.Identifier.Text);
        Assert.Empty(interfaceDeclaration.Fields);
    }
    
    [Fact]
    public void Parses_Interfaces()
    {
        var tree = Parse("interface Abc { a: int; mut b: float; }");
        var statement = tree.Statements.First();
        Assert.IsType<InterfaceDeclaration>(statement);
        
        var interfaceDeclaration = (InterfaceDeclaration)statement;
        Assert.Equal(SyntaxKind.InterfaceKeyword, interfaceDeclaration.Keyword.Kind);
        Assert.Equal("Abc", interfaceDeclaration.Identifier.Text);
        Assert.Equal(2, interfaceDeclaration.Fields.Count);
        
        var fieldA = interfaceDeclaration.Fields.First();
        var fieldB = interfaceDeclaration.Fields.Last();
        Assert.Equal("a", fieldA.Identifier.Text);
        Assert.False(fieldA.IsMutable);
        Assert.IsType<SingularType>(fieldA.Type);
        Assert.Equal("b", fieldB.Identifier.Text);
        Assert.True(fieldB.IsMutable);
        Assert.IsType<SingularType>(fieldB.Type);
        
        var aType = (SingularType)fieldA.Type;
        var bType = (SingularType)fieldB.Type;
        Assert.Equal("int", aType.Token.Text);
        Assert.Equal("float", bType.Token.Text);
    }

    [Fact]
    public void Parses_WhileStatements()
    {
        const string input = """
                             while i < 10
                                ++i;
                             """;
        
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<While>(statement);
        
        var whileStatement = (While)statement;
        Assert.IsType<BinaryOp>(whileStatement.Condition);
        Assert.IsType<ExpressionStatement>(whileStatement.Body);
        
        var conditionBinaryOp = (BinaryOp)whileStatement.Condition;
        Assert.IsType<IdentifierName>(conditionBinaryOp.Left);
        Assert.IsType<Literal>(conditionBinaryOp.Right);

        var body = (ExpressionStatement)whileStatement.Body;
        Assert.IsType<UnaryOp>(body.Expression);
    }

    [Fact]
    public void Parses_IfStatements()
    {
        const string input = """
                             if a
                                69 + 420;
                             else if b
                                420 - 69;
                             else
                                69;
                             """;
        
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<If>(statement);
        
        var ifStatement = (If)statement;
        Assert.IsType<IdentifierName>(ifStatement.Condition);
        Assert.IsType<ExpressionStatement>(ifStatement.Body);
        Assert.IsType<If>(ifStatement.ElseBranch);
        
        var mainCondition = (IdentifierName)ifStatement.Condition;
        Assert.Equal("a", mainCondition.Token.Text);
        
        var mainBody = (ExpressionStatement)ifStatement.Body;
        Assert.IsType<BinaryOp>(mainBody.Expression);
        
        var elseIf = (If)ifStatement.ElseBranch;
        Assert.IsType<IdentifierName>(elseIf.Condition);
        Assert.IsType<ExpressionStatement>(elseIf.Body);
        Assert.IsType<ExpressionStatement>(elseIf.ElseBranch);
        
        var elseIfCondition = (IdentifierName)elseIf.Condition;
        Assert.Equal("b", elseIfCondition.Token.Text);
        
        var elseIfBody = (ExpressionStatement)elseIf.Body;
        Assert.IsType<BinaryOp>(elseIfBody.Expression);
        
        var elseBranch = (ExpressionStatement)elseIf.ElseBranch;
        Assert.IsType<Literal>(elseBranch.Expression);
    }

    [Theory]
    [InlineData("fn add(x: int, y = 1): int -> x + y;", true)]
    [InlineData("fn add(x: int, y: int = 1): int { return x + y; }", false)]
    public void Parses_FunctionDeclarations_WithParameters(string input, bool noTypeOnY)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<FunctionDeclaration>(statement);
        
        var functionDeclaration = (FunctionDeclaration)statement;
        Assert.Equal(SyntaxKind.FnKeyword, functionDeclaration.Keyword.Kind);
        Assert.Equal("add", functionDeclaration.Name.Token.Text);
        Assert.Equal(2, functionDeclaration.Parameters.Count);
        
        var xParameter = functionDeclaration.Parameters.First();
        var yParameter = functionDeclaration.Parameters.Last();
        Assert.Equal("x", xParameter.Name.Token.Text);
        Assert.Equal("y", yParameter.Name.Token.Text);
        Assert.IsType<SingularType>(xParameter.Type);
        Assert.Null(xParameter.Initializer);
        if (noTypeOnY)
            Assert.Null(yParameter.Type);
        
        Assert.NotNull(yParameter.Initializer);
        Assert.Equal(1, yParameter.Initializer.Token.Value);
        
        var xType = (SingularType)xParameter.Type;
        Assert.Equal("int", xType.Token.Text);

        Assert.Single(functionDeclaration.Body.Statements);
        Assert.IsType<Return>(functionDeclaration.Body.Statements.First());
        
        var returnStatement = (Return)functionDeclaration.Body.Statements.First();
        Assert.IsType<BinaryOp>(returnStatement.Expression);
        
        var binaryOp = (BinaryOp)returnStatement.Expression;
        Assert.Equal("+", binaryOp.Operator.Text);
        Assert.IsType<IdentifierName>(binaryOp.Left);
        Assert.IsType<IdentifierName>(binaryOp.Right);

        var left = (IdentifierName)binaryOp.Left;
        var right = (IdentifierName)binaryOp.Right;
        Assert.Equal("x", left.Token.Text);
        Assert.Equal("y", right.Token.Text);
    }

    [Theory]
    [InlineData("fn abc: int -> 123;")]
    [InlineData("fn abc: int { return 123; }")]
    public void Parses_FunctionDeclarations(string input)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<FunctionDeclaration>(statement);
        
        var functionDeclaration = (FunctionDeclaration)statement;
        Assert.Equal(SyntaxKind.FnKeyword, functionDeclaration.Keyword.Kind);
        Assert.Equal("abc", functionDeclaration.Name.Token.Text);
        Assert.Empty(functionDeclaration.Parameters);
        Assert.IsType<SingularType>(functionDeclaration.ReturnType);
        
        var returnType = (SingularType)functionDeclaration.ReturnType;
        Assert.Equal("int", returnType.Token.Text);

        Assert.Single(functionDeclaration.Body.Statements);
        Assert.IsType<Return>(functionDeclaration.Body.Statements.First());
        
        var returnStatement = (Return)functionDeclaration.Body.Statements.First();
        Assert.IsType<Literal>(returnStatement.Expression);
        
        var literal = (Literal)returnStatement.Expression;
        Assert.Equal(123, literal.Token.Value);
    }
    
    [Fact]
    public void Parses_MemberAccess()
    {
        var tree = Parse("abc.bb;");
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<MemberAccess>(expressionStatement.Expression);
        
        var memberAccess = (MemberAccess)expressionStatement.Expression;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        
        var expressionName = (IdentifierName)memberAccess.Expression;
        Assert.Equal("abc", expressionName.Token.Text);
        Assert.Equal("bb", memberAccess.Name.Token.Text);
    }
    
    [Fact]
    public void Parses_ElementAccess()
    {
        var tree = Parse("abc[\"bb\"];");
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<ElementAccess>(expressionStatement.Expression);
        
        var elementAccess = (ElementAccess)expressionStatement.Expression;
        Assert.IsType<IdentifierName>(elementAccess.Expression);
        
        var expressionName = (IdentifierName)elementAccess.Expression;
        Assert.Equal("abc", expressionName.Token.Text);
        Assert.IsType<Literal>(elementAccess.IndexExpression);
        
        var indexExpression = (Literal)elementAccess.IndexExpression;
        Assert.Equal("bb", indexExpression.Token.Value);
    }

    [Theory]
    [InlineData("abc();", 0)]
    [InlineData("abc(69);", 1)]
    [InlineData("abc(69, 420);", 2)]
    public void Parses_Invocation(string input, int expectedArgumentCount)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<Invocation>(expressionStatement.Expression);
        
        var invocation = (Invocation)expressionStatement.Expression;
        Assert.IsType<IdentifierName>(invocation.Callee);
        
        var calleeName = (IdentifierName)invocation.Callee;
        Assert.Equal("abc", calleeName.Token.Text);
        Assert.Equal(expectedArgumentCount, invocation.Arguments.Count);
        foreach (var argument in invocation.Arguments)
            Assert.IsType<Literal>(argument);
    }

    [Fact]
    public void Parses_ReturnStatements()
    {
        var tree = Parse("return 123;");
        var statement = tree.Statements.First();
        Assert.IsType<Return>(statement);
        
        var returnStatement = (Return)statement;
        Assert.Equal(SyntaxKind.ReturnKeyword, returnStatement.Keyword.Kind);
        Assert.IsType<Literal>(returnStatement.Expression);
        
        var literal = (Literal)returnStatement.Expression;
        Assert.Equal(123, literal.Token.Value);
    }
    
    [Fact]
    public void Parses_NullableTypes()
    {
        var tree = Parse("let y: int? = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.Equal("y", declaration.Name.Token.Text);
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<UnionType>(declaration.Type);

        var unionType = (UnionType)declaration.Type;
        Assert.IsType<SingularType>(unionType.Types.First());
        Assert.IsType<SingularType>(unionType.Types.Last());

        var intType = (SingularType)unionType.Types.First();
        var noneType = (SingularType)unionType.Types.Last();
        Assert.Equal("int", intType.Token.Text);
        Assert.Equal("none", noneType.Token.Text);
    }
    
    [Fact]
    public void Parses_FunctionTypes()
    {
        var tree = Parse("let y: (a: int, b: char) -> bool;");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.Equal("y", declaration.Name.Token.Text);
        Assert.False(declaration.IsMutable);
        Assert.Null(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<FunctionType>(declaration.Type);

        var functionType = (FunctionType)declaration.Type;
        Assert.Equal(2, functionType.ParameterTypes.Count);
        Assert.IsType<SingularType>(functionType.ReturnType);

        var returnType = (SingularType)functionType.ReturnType;
        Assert.Equal("bool", returnType.Token.Text);
        
        var intType = (SingularType)functionType.ParameterTypes.Values.First();
        var charType = (SingularType)functionType.ParameterTypes.Values.Last();
        Assert.Equal("int", intType.Token.Text);
        Assert.Equal("char", charType.Token.Text);
    }

    [Fact]
    public void Parses_UnionAndIntersectionTypes()
    {
        var tree = Parse("let y: int | char & string = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.Equal("y", declaration.Name.Token.Text);
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<UnionType>(declaration.Type);

        var unionType = (UnionType)declaration.Type;
        Assert.IsType<SingularType>(unionType.Types.First());
        Assert.IsType<IntersectionType>(unionType.Types.Last());

        var intType = (SingularType)unionType.Types.First();
        var charAndStringType = (IntersectionType)unionType.Types.Last();
        Assert.IsType<SingularType>(charAndStringType.Types.First());
        Assert.IsType<SingularType>(charAndStringType.Types.Last());
        
        var charType = (SingularType)charAndStringType.Types.First();
        var stringType = (SingularType)charAndStringType.Types.Last();
        Assert.Equal("int", intType.Token.Text);
        Assert.Equal("char", charType.Token.Text);
        Assert.Equal("string", stringType.Token.Text);
    }
    
    [Fact]
    public void Parses_ParenthesizedTypes()
    {
        var tree = Parse("let x: (int) = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.Equal("x", declaration.Name.Token.Text);
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<ParenthesizedType>(declaration.Type);

        var type = (ParenthesizedType)declaration.Type;
        Assert.IsType<SingularType>(type.Type);
        
        var singularType = (SingularType)type.Type;
        Assert.Equal("int", singularType.Token.Text);
    }

    [Fact]
    public void Parses_VariableDeclarations()
    {
        var tree = Parse("let x: int = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.Equal("x", declaration.Name.Token.Text);
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<SingularType>(declaration.Type);

        var type = (SingularType)declaration.Type;
        Assert.Equal("int", type.Token.Text);
    }

    [Fact]
    public void Parses_MutableVariableDeclarations()
    {
        var tree = Parse("let mut x = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.Equal("x", declaration.Name.Token.Text);
        Assert.True(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.IsType<Literal>(declaration.Initializer);
    }
    
    [Theory]
    [InlineData("abc!", SyntaxKind.Bang)]
    public void Parses_PostfixOperators(string input, SyntaxKind operatorKind)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<PostfixOp>(node);

        var postfixOperation = (UnaryOp)node;
        Assert.Equal(operatorKind, postfixOperation.Operator.Kind);
    }

    [Theory]
    [InlineData("!true", SyntaxKind.Bang)]
    [InlineData("++a", SyntaxKind.PlusPlus)]
    [InlineData("--b", SyntaxKind.MinusMinus)]
    [InlineData("-10", SyntaxKind.Minus)]
    [InlineData("~14", SyntaxKind.Tilde)]
    public void Parses_UnaryOperators(string input, SyntaxKind operatorKind)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<UnaryOp>(node);

        var unaryOperation = (UnaryOp)node;
        Assert.Equal(operatorKind, unaryOperation.Operator.Kind);
    }

    [Theory]
    [InlineData("1 + 2", SyntaxKind.Plus)]
    [InlineData("3 - 2", SyntaxKind.Minus)]
    [InlineData("3 * 4", SyntaxKind.Star)]
    [InlineData("6 % 4", SyntaxKind.Percent)]
    [InlineData("10 ^ 2", SyntaxKind.Carat)]
    [InlineData("2 & 7", SyntaxKind.Ampersand)]
    [InlineData("9 | 4", SyntaxKind.Pipe)]
    [InlineData("5 ~ 3", SyntaxKind.Tilde)]
    [InlineData("true && false", SyntaxKind.AmpersandAmpersand)]
    [InlineData("true || false", SyntaxKind.PipePipe)]
    public void Parses_BinaryOperators(string input, SyntaxKind operatorKind)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<BinaryOp>(node);

        var binaryOperation = (BinaryOp)node;
        Assert.IsType<Literal>(binaryOperation.Left);
        Assert.IsType<Literal>(binaryOperation.Right);
        Assert.Equal(operatorKind, binaryOperation.Operator.Kind);
    }

    [Theory]
    [InlineData("a = 2", SyntaxKind.Equals)]
    [InlineData("a += 2", SyntaxKind.PlusEquals)]
    [InlineData("a //= 2", SyntaxKind.SlashSlashEquals)]
    public void Parses_AssignmentOperators(string input, SyntaxKind operatorKind)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<AssignmentOp>(node);

        var assignmentOperation = (AssignmentOp)node;
        Assert.Equal(operatorKind, assignmentOperation.Operator.Kind);
    }

    [Fact]
    public void Parses_OperatorPrecedence()
    {
        {
            var tree = Parse("3 ^ 2 * 4 - 2");
            var statement = tree.Statements.First();
            Assert.IsType<ExpressionStatement>(statement);

            var node = ((ExpressionStatement)statement).Expression;
            Assert.IsType<BinaryOp>(node);

            var subtraction = (BinaryOp)node;
            Assert.IsType<BinaryOp>(subtraction.Left);
            var multiplication = (BinaryOp)subtraction.Left;
            Assert.IsType<Literal>(subtraction.Right);
            var twoLiteral = (Literal)subtraction.Right;
            Assert.IsType<BinaryOp>(multiplication.Left);
            var exponentation = (BinaryOp)multiplication.Left;
            Assert.IsType<Literal>(multiplication.Right);
            var fourLiteral = (Literal)multiplication.Right;

            Assert.Equal(SyntaxKind.Minus, subtraction.Operator.Kind);
            Assert.Equal(SyntaxKind.Star, multiplication.Operator.Kind);
            Assert.Equal(SyntaxKind.Carat, exponentation.Operator.Kind);
            Assert.Equal(2, twoLiteral.Token.Value);
            Assert.Equal(4, fourLiteral.Token.Value);
        }
        {
            var tree = Parse("true || false && true");
            var statement = tree.Statements.First();
            Assert.IsType<ExpressionStatement>(statement);

            var node = ((ExpressionStatement)statement).Expression;
            Assert.IsType<BinaryOp>(node);

            var or = (BinaryOp)node;
            Assert.IsType<Literal>(or.Left);
            var trueLiteral = (Literal)or.Left;
            Assert.IsType<BinaryOp>(or.Right);
            var and = (BinaryOp)or.Right;
            Assert.IsType<Literal>(and.Left);
            Assert.IsType<Literal>(and.Right);

            Assert.Equal(SyntaxKind.PipePipe, or.Operator.Kind);
            Assert.Equal(SyntaxKind.AmpersandAmpersand, and.Operator.Kind);
            Assert.Equal(true, trueLiteral.Token.Value);
        }
        {
            var tree = Parse("x += y * z");
            var statement = tree.Statements.First();
            Assert.IsType<ExpressionStatement>(statement);

            var node = ((ExpressionStatement)statement).Expression;
            Assert.IsType<AssignmentOp>(node);

            var assignment = (AssignmentOp)node;
            Assert.IsType<IdentifierName>(assignment.Left);
            var target = (IdentifierName)assignment.Left;
            Assert.IsType<BinaryOp>(assignment.Right);
            var multiplication = (BinaryOp)assignment.Right;
            Assert.IsType<IdentifierName>(multiplication.Left);
            Assert.IsType<IdentifierName>(multiplication.Right);

            Assert.Equal(SyntaxKind.PlusEquals, assignment.Operator.Kind);
            Assert.Equal(SyntaxKind.Star, multiplication.Operator.Kind);
        }
    }
    
    [Theory]
    [InlineData("{ a: true }", SyntaxKind.StringLiteral, 1)]
    [InlineData("{ [\"a\"]: true }", SyntaxKind.StringLiteral, 1)]
    [InlineData("{ [1]: true }", SyntaxKind.IntLiteral, 1)]
    [InlineData("{}", SyntaxKind.Comma, 0)] // syntaxkind doesnt matter
    public void Parses_ObjectLiterals(string input, SyntaxKind keyLiteralKind, int propertyCount)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<ObjectLiteral>(node);

        var objectLiteral = (ObjectLiteral)node;
        Assert.Equal(propertyCount, objectLiteral.Properties.Count);

        if (objectLiteral.Properties.Count > 0)
        {
            var key = objectLiteral.Properties.Keys.First();
            var value = objectLiteral.Properties.Values.First();
            Assert.IsType<Literal>(key);
            Assert.IsType<Literal>(value);

            var keyLiteral = (Literal)key;
            var valueLiteral = (Literal)value;
            Assert.Equal(keyLiteralKind, keyLiteral.Token.Kind);
            Assert.Equal(SyntaxKind.BoolLiteral, valueLiteral.Token.Kind);
        }
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
    public void Parses_Literals(string input)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Literal>(node);

        var literal = (Literal)node;
        Assert.Equal(input, literal.Token.Text);
    }

    [Fact]
    public void Parses_ParenthesizedExpressions()
    {
        var tree = Parse("(1 + 2)");
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Parenthesized>(node);

        var parenthesized = (Parenthesized)node;
        Assert.IsType<BinaryOp>(parenthesized.Expression);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc123")]
    [InlineData("abc_123")]
    public void Parses_Identifiers(string input)
    {
        var tree = Parse(input);
        var statement = tree.Statements.First();
        Assert.IsType<ExpressionStatement>(statement);

        var node = ((ExpressionStatement)statement).Expression;
        Assert.IsType<IdentifierName>(node);

        var identifier = (IdentifierName)node;
        Assert.Equal(input, identifier.Token.Text);
    }
}
