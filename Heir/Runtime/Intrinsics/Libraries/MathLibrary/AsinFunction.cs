using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AsinFunction()
    : IntrinsicFunction(
        "asin",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Asin(Convert.ToDouble(args.First()));
}