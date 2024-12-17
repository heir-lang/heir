using static Heir.Tests.Common;

namespace Heir.Tests;

public class VirtualMachineTest
{
    [Theory]
    [InlineData("let mut x = 1; ++x;", 1.0)]
    [InlineData("let mut y = 2; --y;", 2.0)]
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
    [InlineData("let x = 1;", "x", 1L)]
    [InlineData("let mut y: int = 2;", "y", 2L)]
    public void Evaluates_VariableDeclarations(string input, string name, object? expectedValue)
    {
        var (_, vm) = Evaluate(input);
        Assert.True(vm.Scope.IsDeclared(name));
        Assert.True(vm.Scope.IsDefined(name));
        Assert.Equal(expectedValue, vm.Scope.Lookup(name));
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
