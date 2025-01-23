using static Heir.Tests.Common;

namespace Heir.Tests;

public class TypeCheckerTest
{
    [Theory]
    [InlineData("fn abc(x = 1) -> none; abc(1, 2);", DiagnosticCode.H019)]
    [InlineData("fn abc(x: int = 1) -> none; abc(1, 2);", DiagnosticCode.H019)]
    [InlineData("fn abc(x: int) -> none; abc(1, 2);", DiagnosticCode.H019)]
    [InlineData("fn abc(x: int) -> none; abc();", DiagnosticCode.H019)]
    [InlineData("fn abc -> none; abc(1);", DiagnosticCode.H019)]
    [InlineData("1[1]", DiagnosticCode.H018)]
    [InlineData("1()", DiagnosticCode.H018)]
    [InlineData("let foo = {}; foo.a", DiagnosticCode.H013)]
    [InlineData("let mut x = 1; x = 'a'", DiagnosticCode.H007)]
    [InlineData("fn abc: int -> 'a'", DiagnosticCode.H007)]
    [InlineData("fn abc(x: int) {} abc('a')", DiagnosticCode.H007)]
    [InlineData("let foo = {}; foo['a']", DiagnosticCode.H007)]
    public void ThrowsWith(string input, DiagnosticCode expectedDiagnosticCode)
    {
        var diagnostics = TypeCheck(input);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedDiagnosticCode);
    }

    [Theory]
    [InlineData("let mut x: int | char = 1; x = 'a';")]
    [InlineData("let foo = { bar: \"baz\" }; foo[\"bar\"];")]
    [InlineData("let foo = { bar: \"baz\" }; foo.bar;")]
    [InlineData("let foo = { bar: { baz: 69 } }; foo.bar.baz;")]
    [InlineData("({ bar: 69 }).bar;")]
    [InlineData("fn brah -> { a: \"brah\" }; let foo = { bar: { baz: brah } }; foo[\"bar\"].baz()[\"a\"];")]

    public void DoesNotThrowWith(string input)
    {
        var diagnostics = TypeCheck(input);
        Assert.Empty(diagnostics);
    }
}
