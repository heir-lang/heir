using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class InDegreesFunction()
    : IntrinsicFunction(
        "inDegrees",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Convert.ToDouble(args.First()) * 180 / Math.PI;
}