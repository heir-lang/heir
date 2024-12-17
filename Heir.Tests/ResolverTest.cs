using static Heir.Tests.Common;

namespace Heir.Tests
{
    public class ResolverTest
    {
        [Theory]
        [InlineData("x", DiagnosticCode.H011)]
        [InlineData("let x = x;", DiagnosticCode.H010)]
        [InlineData("let x = 1; let x = 2;", DiagnosticCode.H009)]
        public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
        {
            var diagnostics = Resolve(input);
            Assert.True(diagnostics.HasErrors());

            Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
        }
    }
}
