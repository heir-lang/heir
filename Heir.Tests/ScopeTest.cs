using Xunit.Sdk;

namespace Heir.Tests
{
    public class ScopeTest
    {
        [Fact]
        public void DeclaresVariables()
        {
            var scope = new Scope(new());
            var name = "x";
            scope.Define(name, null);
            Assert.True(scope.IsDeclared(name));
            Assert.False(scope.IsDefined(name));
            Assert.Null(scope.Lookup(name));
        }

        [Fact]
        public void DefinesVariables()
        {
            var scope = new Scope(new());
            var name = "x";
            var value = 123;
            scope.Define(name, 123);
            Assert.True(scope.IsDeclared(name));
            Assert.True(scope.IsDefined(name));
            Assert.Equal(value, scope.Lookup(name));
        }

        [Fact]
        public void LooksUpVariables()
        {
            var diagnostics = new DiagnosticBag();
            var scope = new Scope(diagnostics);
            var localScope = new Scope(diagnostics, scope);
            var name = "x";
            var value = 123;
            scope.Define(name, 123);
            Assert.True(localScope.IsDeclared(name));
            Assert.True(localScope.IsDefined(name));
            Assert.Equal(value, localScope.Lookup(name));
        }
    }
}
