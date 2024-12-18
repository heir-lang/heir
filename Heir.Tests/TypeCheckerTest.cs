using static Heir.Tests.Common;

namespace Heir.Tests;

public class TypeCheckerTest
{
    [Theory]
    [InlineData("let mut x = 1; x = 'a'")]
    public void ThrowsWith(string input)
    {
        var diagnostics = TypeCheck(input);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == DiagnosticCode.H007);
    }
}
