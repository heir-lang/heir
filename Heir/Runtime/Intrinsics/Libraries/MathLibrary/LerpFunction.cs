using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class LerpFunction()
    : IntrinsicFunction(
        "lerp",
        new()
        {
            { "a", IntrinsicTypes.Number },
            { "b", IntrinsicTypes.Number },
            { "t", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args =>
    {
        var a = Convert.ToDouble(args[0]);
        var b = Convert.ToDouble(args[1]);
        var t = Convert.ToDouble(args[2]);
        
        return a + t * (b - a);
    };
}