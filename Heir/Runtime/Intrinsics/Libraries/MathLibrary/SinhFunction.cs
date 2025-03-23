using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class SinhFunction()
    : IntrinsicFunction(
        "sinh",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Sinh(Convert.ToDouble(args.First()));
}