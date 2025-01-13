using Heir.Runtime;
using Heir.Runtime.Values;
using static Heir.Tests.Common;

namespace Heir.Tests;

public class VirtualMachineTest
{
    [Theory]
    [InlineData("let mut x = 1; ++x;", 2.0)]
    [InlineData("let mut y = 2; --y;", 1.0)]
    [InlineData("let mut z = 1; ++z; z;", 2.0)]
    public void Evaluates_IncrementDecrement(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("let mut x = 1; x = 2;", 2L)]
    [InlineData("let mut y = 5; y += 5;", 10.0)]
    [InlineData("let mut z = 9; z //= 2;", 4L)]
    public void Evaluates_Assignment(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("fn double(n: int) -> n * 2; double(10);", 20.0)]
    [InlineData("fn increment(n: int, amount = 1) -> n + amount; increment(5);", 6.0)]
    [InlineData("fn increment(n: int, amount = 1) -> n + amount; increment(5, 5);", 10.0)]
    [InlineData("fn say_hello(name: string): string { return \"hello, \" + name + \"!\" } say_hello(\"johnny\");", "hello, johnny!")]
    public void Evaluates_Invocation(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }
    
    [Fact]
    public void Evaluates_FunctionDeclarations()
    {
        const string name = "abc";
        var (resultValue, vm) = Evaluate($"fn {name}(x: int) -> 123 + x;");
        Assert.True(vm.Scope.IsDeclared(name));
        Assert.True(vm.Scope.IsDefined(name));
        Assert.Null(resultValue);
        
        var value = vm.Scope.Lookup(name);
        Assert.IsType<Function>(value);
        
        var function = (Function)value;
        Assert.Equal(name, function.Name);
    }

    [Theory]
    [InlineData("let x = 1;", "x", 1L)]
    [InlineData("let mut y: int = 2;", "y", 2L)]
    public void Evaluates_VariableDeclarations(string input, string name, object? expectedValue)
    {
        var (resultValue, vm) = Evaluate(input);
        Assert.True(vm.Scope.IsDeclared(name));
        Assert.True(vm.Scope.IsDefined(name));
        Assert.Equal(expectedValue, vm.Scope.Lookup(name));
        Assert.Null(resultValue);
    }

    [Theory]
    [InlineData("{ a: true }", "a", true)]
    [InlineData("{ [\"a\"]: 69 }", "a", 69L)]
    [InlineData("{ [1]: 420 }", 1L, 420L)]
    public void Evaluates_ObjectLiterals(string input, object expectedKey, object? expectedValue)
    {
        var (resultValue, _) = Evaluate(input);
        Assert.IsType<ObjectValue>(resultValue);

        var objectValue = (ObjectValue)resultValue;
        var key = objectValue.Keys.First();
        var value = objectValue.Values.First();
        Assert.Equal(expectedKey, key);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void Evaluates_EmptyObjectLiterals()
    {
        var (resultValue, _) = Evaluate("{}");
        Assert.IsType<ObjectValue>(resultValue);

        var objectValue = (ObjectValue)resultValue;
        Assert.Empty(objectValue);
    }

    [Theory]
    [InlineData("69.420", 69.420)]
    [InlineData("0x3E", 62L)]
    [InlineData("0o342", 226L)]
    [InlineData("0b11", 3L)]
    [InlineData("\"hello\"", "hello")]
    [InlineData("'a'", 'a')]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("none", null)]
    public void Evaluates_Literals(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("3 + 2", 5.0)]
    [InlineData("9 - 3", 6.0)]
    [InlineData("3 * 3", 9.0)]
    [InlineData("10 / 2", 5.0)]
    [InlineData("9 // 2", 4L)]
    [InlineData("9 % 2", 1.0)]
    [InlineData("14 << 1", 28L)]
    [InlineData("11 & 7", 3L)]
    [InlineData("4 | 9", 13L)]
    [InlineData("5 ~ 3", 6L)]
    [InlineData("~7", -8L)]
    [InlineData("-5", -5.0)]
    [InlineData("3 * 2 + 1", 7.0)]
    [InlineData("3 * (2 + 1)", 9.0)]
    public void Evaluates_Arithmetic(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("false || true", true)]
    [InlineData("true && false || true", true)]
    [InlineData("false && true || false", false)]
    [InlineData("!false", true)]
    [InlineData("!!false", false)]
    public void Evaluates_Logical(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("3 == 2", false)]
    [InlineData("3 != 2", true)]
    [InlineData("3 == 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("3 <= 3", true)]
    [InlineData("3 >= 3", true)]
    [InlineData("3 < 3", false)]
    [InlineData("3 > 3", false)]
    [InlineData("4 > 3", true)]
    [InlineData("3 < 4", true)]
    [InlineData("3 <= 4", true)]
    [InlineData("4 >= 3", true)]
    [InlineData("3 <= 2", false)]
    [InlineData("2 >= 3", false)]
    public void Evaluates_Comparison(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }
}
