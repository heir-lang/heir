using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class CbrtFunction()
    : IntrinsicFunction(
        "cbrt",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Cbrt(Convert.ToDouble(args.First()));
}