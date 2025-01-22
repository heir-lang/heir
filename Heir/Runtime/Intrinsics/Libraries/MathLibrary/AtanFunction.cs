using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AtanFunction()
    : IntrinsicFunction(
        "atan",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Atan(Convert.ToDouble(args.First()));
}