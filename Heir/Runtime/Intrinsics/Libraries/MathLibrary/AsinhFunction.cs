using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AsinhFunction()
    : IntrinsicFunction(
        "asinh",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Asinh(Convert.ToDouble(args.First()));
}