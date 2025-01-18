using static Heir.Tests.Common;

namespace Heir.Tests;

public class ResolverTest
{
    [Theory]
    [InlineData("x", DiagnosticCode.H011)]
    [InlineData("let x = x;", DiagnosticCode.H010)]
    [InlineData("let x = 1; let x = 2;", DiagnosticCode.H009)]
    [InlineData("return 123;", DiagnosticCode.H015)]
    public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
    {
        var diagnostics = Resolve(input);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
    }
    
    [Theory]
    [InlineData("fn abc: int { return 123; }")]
    public void DoesNotThrowWith(string input)
    {
        var diagnostics = Resolve(input);
        Assert.False(diagnostics.HasErrors);
    }
}
