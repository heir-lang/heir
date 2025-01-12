using Heir.Runtime;

namespace Heir.Tests;

public class ScopeTest
{
    [Fact]
    public void AncestorChain()
    {
        var scope4 = new Scope();
        var scope3 = new Scope(scope4);
        var scope2 = new Scope(scope3);
        var scope1 = new Scope(scope2);
        Assert.NotNull(scope1.Enclosing);
        Assert.StrictEqual(scope2, scope1.Enclosing);
        
        var ancestor1 = scope1.Ancestor(1);
        Assert.NotNull(ancestor1);
        Assert.StrictEqual(scope2, ancestor1);

        var ancestor2 = scope1.Ancestor(2);
        Assert.NotNull(ancestor2);
        Assert.StrictEqual(scope3, ancestor2);
        
        var ancestor3 = scope1.Ancestor(3);
        Assert.NotNull(ancestor3);
        Assert.StrictEqual(scope4, ancestor3);
    }
    
    [Fact]
    public void LooksUpVariables()
    {
        var scope = new Scope();
        var localScope = new Scope(scope);
        const string name = "x";
        const int value = 123;
        
        scope.Define(name, value);
        Assert.Equal(value, scope.Lookup(name));
        Assert.True(localScope.IsDeclared(name));
        Assert.True(localScope.IsDefined(name));
        Assert.Equal(value, localScope.Lookup(name));
    }
    
    [Fact]
    public void LooksUpVariablesAtLevel()
    {
        var scope = new Scope();
        var localScope = new Scope(scope);
        const string name = "x";
        const int value = 123;
        
        scope.Define(name, value);
        Assert.True(localScope.IsDeclared(name));
        Assert.True(localScope.IsDefined(name));
        Assert.Equal(value, localScope.LookupAt(name, 1));
    }
    
    [Fact]
    public void AssignsVariables()
    {
        var scope = new Scope();
        var localScope = new Scope(scope);
        const string name = "x";
        const int value = 123;
        const int newValue = 420;
        
        scope.Define(name, value);
        scope.Assign(name, newValue);
        Assert.Equal(newValue, scope.Lookup(name));
        Assert.True(localScope.IsDeclared(name));
        Assert.True(localScope.IsDefined(name));
        Assert.Equal(newValue, localScope.Lookup(name));
    }
    
    [Fact]
    public void AssignsVariablesAtLevel()
    {
        var scope = new Scope();
        var localScope = new Scope(scope);
        const string name = "x";
        const int value = 123;
        const int newValue = 420;
        
        scope.Define(name, value);
        localScope.AssignAt(name, newValue, 1);
        Assert.Equal(newValue, scope.Lookup(name));
        Assert.True(localScope.IsDeclared(name));
        Assert.True(localScope.IsDefined(name));
        Assert.Equal(newValue, localScope.Lookup(name));
    }
    
    [Fact]
    public void DeclaresVariables()
    {
        var scope = new Scope();
        const string name = "x";
        
        scope.Define(name, null);
        Assert.True(scope.IsDeclared(name));
        Assert.False(scope.IsDefined(name));
        Assert.Null(scope.Lookup(name));
    }

    [Fact]
    public void DefinesVariables()
    {
        var scope = new Scope();
        const string name = "x";
        const int value = 123;
        
        scope.Define(name, value);
        Assert.True(scope.IsDeclared(name));
        Assert.True(scope.IsDefined(name));
        Assert.Equal(value, scope.Lookup(name));
    }
}
