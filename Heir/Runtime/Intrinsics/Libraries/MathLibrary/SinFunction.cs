using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class SinFunction()
    : IntrinsicFunction(
        "sin",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Sin(Convert.ToDouble(args.First()));
}