using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class FloorFunction()
    : IntrinsicFunction(
        "floor",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Int
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Floor(Convert.ToDouble(args.First()));
}