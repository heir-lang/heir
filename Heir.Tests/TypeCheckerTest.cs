using static Heir.Tests.Common;

namespace Heir.Tests;

public class TypeCheckerTest
{
    [Theory]
    [InlineData("1()", DiagnosticCode.H018)]
    [InlineData("let mut x = 1; x = 'a'", DiagnosticCode.H007)]
    [InlineData("fn abc: int -> 'a'", DiagnosticCode.H007)]
    [InlineData("fn abc(x: int) {} abc('a')", DiagnosticCode.H007)]
    public void ThrowsWith(string input, DiagnosticCode expectedDiagnosticCode)
    {
        var diagnostics = TypeCheck(input);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedDiagnosticCode);
    }

    [Theory]
    [InlineData("let mut x: int | char = 1; x = 'a';")]
    public void DoesNotThrowWith(string input)
    {
        var diagnostics = TypeCheck(input);
        Assert.False(diagnostics.HasErrors);
    }
}
