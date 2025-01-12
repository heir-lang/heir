namespace Heir.Tests;

public class ScopeTest
{
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
