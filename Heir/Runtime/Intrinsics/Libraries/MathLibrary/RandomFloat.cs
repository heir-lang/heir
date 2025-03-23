using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class RandomFloatFunction()
    : IntrinsicFunction(
        "randomFloat",
        new()
        {
            { "minimum", IntrinsicTypes.Number },
            { "maximum", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    private static readonly Random _random = new();
    
    public override BaseDelegate Invoke { get; } = args =>
    {
        var minimum = Convert.ToDouble(args[0]);
        var maximum = Convert.ToDouble(args[1]);
        return minimum + _random.NextDouble() * (maximum - minimum);
    };
}