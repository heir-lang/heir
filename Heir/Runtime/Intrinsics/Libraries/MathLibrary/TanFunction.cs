using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class TanFunction()
    : IntrinsicFunction(
        "tan",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Tan(Convert.ToDouble(args.First()));
}