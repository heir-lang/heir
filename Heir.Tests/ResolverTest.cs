using Heir.Diagnostics;
using static Heir.Tests.Common;

namespace Heir.Tests;

public class ResolverTest
{
    [Theory]
    [InlineData("break;", DiagnosticCode.H023)]
    [InlineData("continue;", DiagnosticCode.H023)]
    [InlineData("return 123;", DiagnosticCode.H015)]
    [InlineData("x", DiagnosticCode.H011)]
    [InlineData("let x = x;", DiagnosticCode.H010)]
    [InlineData("let x = 1; let x = 2;", DiagnosticCode.H009)]
    public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
    {
        var diagnostics = Resolve(input);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
    }
    
    [Theory]
    [InlineData("fn abc: int { return 123; }")]
    [InlineData("let a: int; a;")]
    public void DoesNotThrowWith(string input)
    {
        var diagnostics = Resolve(input);
        Assert.Empty(diagnostics);
    }
}
