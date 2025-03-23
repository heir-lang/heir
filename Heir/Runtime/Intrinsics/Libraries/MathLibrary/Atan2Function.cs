using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class Atan2Function()
    : IntrinsicFunction(
        "atan2",
        new()
        {
            { "y", IntrinsicTypes.Number },
            { "x", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args =>
        Math.Atan2(Convert.ToDouble(args.First()), Convert.ToDouble(args.Last()));
}