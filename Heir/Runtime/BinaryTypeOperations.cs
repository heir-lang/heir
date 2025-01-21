using Heir.CodeGeneration;

namespace Heir.Runtime;

public static class BinaryTypeOperations
{
    public static readonly Dictionary<OpCode, Func<double, double, object?>> Double = new()
    {
        { OpCode.ADD, (a, b) => a + b },
        { OpCode.SUB, (a, b) => a - b },
        { OpCode.MUL, (a, b) => a * b },
        { OpCode.DIV, (a, b) => a / b },
        { OpCode.IDIV, (a, b) => Convert.ToInt64(Math.Floor(a / b)) },
        { OpCode.MOD, (a, b) => a % b },
        { OpCode.POW, (a, b) => Math.Pow(a, b) },
        { OpCode.LT, (a, b) => a < b },
        { OpCode.LTE, (a, b) => a <= b },
        { OpCode.GT, (a, b) => a > b },
        { OpCode.GTE, (a, b) => a >= b }
    };

    public static readonly Dictionary<OpCode, Func<long, long, object?>> Long = new()
    {
        { OpCode.BAND, (a, b) => a & b },
        { OpCode.BOR, (a, b) => a | b },
        { OpCode.BXOR, (a, b) => a ^ b }
    };
    
    public static readonly Dictionary<OpCode, Func<int, int, object?>> Int = new()
    {
        { OpCode.BSHL, (a, b) => a << b },
        { OpCode.BSHR, (a, b) => a >> b }
    };
    
    public static readonly Dictionary<OpCode, Func<bool, bool, bool>> Bool = new()
    {
        { OpCode.AND, (a, b) => a && b },
        { OpCode.OR, (a, b) => a || b }
    };
}