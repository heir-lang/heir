using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class InRadiansFunction()
    : IntrinsicFunction(
        "inRadians",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Convert.ToDouble(args.First()) * Math.PI / 180;
}