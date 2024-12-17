using static Heir.Tests.Common;

namespace Heir.Tests
{
    public class ResolverTest
    {
        [Theory]
        [InlineData("x", "H014")]
        [InlineData("let x = x;", "H013")]
        [InlineData("let x = 1; let x = 2;", "H012")]
        public void ThrowsWith(string input, string expectedErrorCode)
        {
            var diagnostics = Resolve(input);
            Assert.True(diagnostics.HasErrors());

            Assert.Contains(diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
        }
    }
}
