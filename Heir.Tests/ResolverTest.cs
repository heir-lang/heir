using static Heir.Tests.Common;

namespace Heir.Tests
{
    public class ResolverTest
    {
        [Theory]
        [InlineData("let x = x;", "H013")]
        [InlineData("x", "H014")]
        public void ThrowsWith(string input, string expectedErrorCode)
        {
            var diagnostics = Resolve(input);
            Assert.True(diagnostics.HasErrors());

            var error = diagnostics.First();
            Assert.Equal(expectedErrorCode, error.Code);
        }
    }
}
