using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class RandomIntFunction()
    : IntrinsicFunction(
        "randomInt",
        new()
        {
            { "minimum", PrimitiveType.Int },
            { "maximum", PrimitiveType.Int }
        },
        PrimitiveType.Int
    )
{
    private static readonly Random _random = new();
    
    public override BaseDelegate Invoke { get; } = args =>
    {
        var minimum = Convert.ToInt32(args[0]);
        var maximum = Convert.ToInt32(args[1]);
        var ratio = Convert.ToDouble(_random.Next()) / Convert.ToDouble(int.MaxValue);
        return Convert.ToInt32(minimum + ratio * (maximum - minimum));
    };
}