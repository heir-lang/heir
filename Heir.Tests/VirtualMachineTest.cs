using Heir.Diagnostics;
using Heir.Runtime.Values;
using static Heir.Tests.Common;

namespace Heir.Tests;

public class VirtualMachineTest
{
    [Theory]
    [InlineData("fn abc -> abc(); abc();", DiagnosticCode.H017)]
    public void ThrowsWith(string input, DiagnosticCode expectedDiagnosticCode)
    {
        var (_, vm) = Evaluate(input);
        Assert.True(vm.Diagnostics.HasErrors);
        Assert.Contains(vm.Diagnostics, diagnostic => diagnostic.Code == expectedDiagnosticCode);
    }
    
    [Theory]
    [InlineData("interface Abc; nameof(Abc);", "Abc")]
    [InlineData("let x = 1; nameof(x);", "x")]
    public void Evaluates_NameOf(string input, string expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void Evaluates_MutableInterfaces()
    {
        const string input = """
                     interface Foo {
                        mut bar: string;
                     }
                     
                     let foo: Foo = { bar: "baz" };
                     foo.bar = "YES";
                     """;
        
        var (value, _) = Evaluate(input);
        Assert.Equal("YES", value);
    }

    [Theory]
    [InlineData("math.randomInt(1, 10)", typeof(int))]
    [InlineData("math.randomFloat(1, 10)", typeof(double))]
    public void Evaluates_PseudorandomFunctions(string input, Type expectedType)
    {
        // test 25 times (more than double the maximum random value) to ensure numbers are within the range
        for (var i = 0; i < 25; i++)
        {
            var (value, vm) = Evaluate(input);
            Assert.Empty(vm.Diagnostics);
            Assert.IsType(expectedType, value);
            Assert.InRange(Convert.ToDouble(value), 1, 10);
        }
    }

    [Theory]
    [InlineData("math.pi", Math.PI)]
    [InlineData("math.e", Math.E)]
    [InlineData("math.tau", Math.Tau)]
    [InlineData("math.inf", double.PositiveInfinity)]
    [InlineData("math.abs(69.0)", 69.0)]
    [InlineData("math.abs(-69.0)", 69.0)]
    [InlineData("math.abs(69)", 69)]
    [InlineData("math.abs(-69)", 69)]
    [InlineData("math.floor(69.9)", 69)]
    [InlineData("math.floor(69.1)", 69)]
    [InlineData("math.ceil(69.9)", 70)]
    [InlineData("math.ceil(69.1)", 70)]
    [InlineData("math.round(69.1)", 69)]
    [InlineData("math.round(69.9)", 70)]
    [InlineData("math.round(69.5)", 70)]
    [InlineData("math.round(69.564, 2)", 69.56)]
    [InlineData("math.round(69.564, 1)", 69.6)]
    [InlineData("math.inRadians(180)", Math.PI)]
    [InlineData("math.inDegrees(math.pi)", 180.0)]
    [InlineData("math.sin(math.pi)", 0.0)]
    [InlineData("math.cos(math.pi)", -1.0)]
    [InlineData("math.tan(math.pi)", 0.0)]
    [InlineData("math.tan(math.inRadians(5))", 0.087488663525)]
    [InlineData("math.asin(1)", Math.PI / 2)]
    [InlineData("math.acos(-1)", Math.PI)]
    [InlineData("math.atan(0)", 0.0)]
    [InlineData("math.atan(-1)", -0.7853981633974483)]
    [InlineData("math.atan2(math.pi, 2)", 1.0038848218538872)]
    [InlineData("math.sinh(math.pi)", 11.548739357257746)]
    [InlineData("math.cosh(math.pi / 2)", 2.5091784786580567)]
    [InlineData("math.tanh(math.pi)", 0.99627207622075)]
    [InlineData("math.asinh(math.pi)", 1.8622957433108482)]
    [InlineData("math.acosh(math.pi / 2)", 1.0232274785475506)]
    [InlineData("math.atanh(0.5)", 0.5493061443340549)]
    [InlineData("math.sqrt(25)", 5.0)]
    [InlineData("math.cbrt(125)", 5.0)]
    [InlineData("math.q_rsqrt(25)", 0.2, 0.001)]
    [InlineData("math.q_rsqrt(25, true)", 0.2, 0.000001)]
    [InlineData("math.lerp(0, 10, 0.5)", 5.0)]
    public void Evaluates_MathLibrary<T>(string input, T expectedValue, double tolerance = 1e-10)
    {
        var (value, vm) = Evaluate(input);
        Assert.Empty(vm.Diagnostics);
        
        switch (expectedValue)
        {
            case double expectedDouble when value is double actualDouble:
                AssertExtensions.FuzzyEqual(expectedDouble, actualDouble, tolerance);
                break;
            case int expectedInt when value is int actualInt:
                AssertExtensions.FuzzyEqual(expectedInt, actualInt, tolerance);
                break;
            
            default:
                throw new Exception($"Expected value & actual value type mismatch\nExpected type: {expectedValue?.GetType().ToString() ?? "null"}\nActual type: {value?.GetType().ToString() ?? "null"}");
        }
    }

    [Fact]
    public void Evaluates_NowIntrinsicFunction()
    {
        var (value, _) = Evaluate("now()");
        Assert.IsType<double>(value);
    }
    
    [Fact]
    public void Evaluates_WhileStatements()
    {
        const string input = """
                     let mut i = 0;
                     let mut x = 0;
                     
                     while ++i <= 10
                         x += i;
                         
                     x;
                     """;
        
        var (value, _) = Evaluate(input);
        Assert.Equal(55.0, value);
    }

    [Theory]
    [InlineData("1", 2.0)]
    [InlineData("2", 4.0)]
    [InlineData("3", 15.0)]
    public void Evaluates_IfStatements(string xValue, double expectedValue)
    {
        var input = $"""
                     let x = {xValue};
                     if x == 1
                         x + 1;
                     else if x == 2
                         x + 2;
                     else
                         x * 5;
                     """;
        
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }
    
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
    [InlineData("let mut x = 1; x = 2;", 2)]
    [InlineData("let mut y = 5; y += 5;", 10.0)]
    [InlineData("let mut z = 9; z //= 2;", 4)]
    public void Evaluates_Assignment(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }
    
    [Theory] 
    [InlineData("let foo = { bar: \"baz\" }; foo.bar;", "baz")]
    [InlineData("let foo = { bar: \"baz\", boof: \"rah\" }; foo.boof;", "rah")]
    [InlineData("({ abc: \"def\" }).abc;", "def")]
    [InlineData("let a = { b: { c: 123 } }; a.b.c;", 123)]
    [InlineData("fn brah -> { a: 123 }; let foo = { bar: brah }; foo.bar().a;", 123)]
    public void Evaluates_MemberAccess(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }
    
    [Theory] 
    [InlineData("let foo = { bar: \"baz\" }; foo[\"bar\"];", "baz")]
    [InlineData("let foo = { bar: \"baz\", boof: \"rah\" }; foo[\"boof\"];", "rah")]
    [InlineData("({ abc: \"def\" })[\"abc\"];", "def")]
    [InlineData("let a = { b: { c: 123 } }; a[\"b\"][\"c\"];", 123)]
    [InlineData("fn brah -> { a: 123 }; let foo = { bar: brah }; foo[\"bar\"]()[\"a\"];", 123)]
    public void Evaluates_ElementAccess(string input, object? expectedValue)
    {
        var (value, _) = Evaluate(input);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("fn fib(n: int) { if n <= 1 return n; return fib(n - 1) + fib(n - 2); } fib(12);", 144.0)]
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
    public void Evaluates_GeneratorFunction()
    {
        const string input = """
                             fn create_greeter(greeting: string) {
                                 fn greet(name: string) ->
                                     greeting + ", " + name + "!";
                                     
                                 return greet;
                             }

                             let greet_english = create_greeter("Hello");
                             greet_english("John");
                             """;
        
        var (value, _) = Evaluate(input);
        Assert.Equal("Hello, John!", value);
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
        Assert.IsType<FunctionValue>(value);
    }

    [Theory]
    [InlineData("let x = 1;", "x", 1)]
    [InlineData("let mut y: int = 2;", "y", 2)]
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
    [InlineData("{ [\"a\"]: 69 }", "a", 69)]
    [InlineData("{ [1]: 420 }", 1, 420)]
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
    [InlineData("0x3E", 62)]
    [InlineData("0o342", 226)]
    [InlineData("0b11", 3)]
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
    [InlineData("9 // 2", 4)]
    [InlineData("9 % 2", 1.0)]
    [InlineData("14 << 1", 28)]
    [InlineData("11 & 7", 3L)]
    [InlineData("4 | 9", 13L)]
    [InlineData("5 ~ 3", 6L)]
    [InlineData("~7", -8L)]
    [InlineData("-5.0", -5.0)]
    [InlineData("-5", -5)]
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
    [InlineData("!!!false", true)]
    [InlineData("!!!!false", false)]
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
