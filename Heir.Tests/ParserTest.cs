using Heir.AST;
using Heir.Syntax;
using static Heir.Tests.Common;

namespace Heir.Tests;

public class ParserTest
{
    [Theory]
    [InlineData("let x", DiagnosticCode.H012)]
    [InlineData("1 = 2", DiagnosticCode.H006B)]
    [InlineData("++1", DiagnosticCode.H006)]
    [InlineData("--3", DiagnosticCode.H006)]
    [InlineData("()", DiagnosticCode.H004D)]
    [InlineData("let", DiagnosticCode.H004C)]
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
        Assert.False(tree.Diagnostics.HasErrors);
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
        Assert.Equal(123L, literal.Token.Value);
    }
    
    [Fact]
    public void Parses_IntersectionTypes()
    {
        var tree = Parse("let y: int | char = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<UnionType>(declaration.Type);

        var unionType = (UnionType)declaration.Type;
        Assert.IsType<SingularType>(unionType.Types.First());
        Assert.IsType<SingularType>(unionType.Types.Last());

        var intType = (SingularType)unionType.Types.First();
        var charType = (SingularType)unionType.Types.Last();
        Assert.Equal("int", intType.Token.Text);
        Assert.Equal("char", charType.Token.Text);
        Assert.Equal("y", declaration.Name.Token.Text);
    }

    [Fact]
    public void Parses_UnionTypes()
    {
        var tree = Parse("let y: int | char & string = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
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
        Assert.Equal("y", declaration.Name.Token.Text);
    }
    
    [Fact]
    public void Parses_ParenthesizedTypes()
    {
        var tree = Parse("let x: (int) = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<ParenthesizedType>(declaration.Type);

        var type = (ParenthesizedType)declaration.Type;
        Assert.IsType<SingularType>(type.Type);
        
        var singularType = (SingularType)type.Type;
        Assert.Equal("int", singularType.Token.Text);
        Assert.Equal("x", declaration.Name.Token.Text);
    }

    [Fact]
    public void Parses_VariableDeclarations()
    {
        var tree = Parse("let x: int = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.False(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.NotNull(declaration.Type);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.IsType<SingularType>(declaration.Type);

        var type = (SingularType)declaration.Type;
        Assert.Equal("int", type.Token.Text);
        Assert.Equal("x", declaration.Name.Token.Text);
    }

    [Fact]
    public void Parses_MutableVariableDeclarations()
    {
        var tree = Parse("let mut x = 1");
        var statement = tree.Statements.First();
        Assert.IsType<VariableDeclaration>(statement);

        var declaration = (VariableDeclaration)statement;
        Assert.True(declaration.IsMutable);
        Assert.NotNull(declaration.Initializer);
        Assert.IsType<Literal>(declaration.Initializer);
        Assert.Equal("x", declaration.Name.Token.Text);
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
            Assert.Equal((long)2, twoLiteral.Token.Value);
            Assert.Equal((long)4, fourLiteral.Token.Value);
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
