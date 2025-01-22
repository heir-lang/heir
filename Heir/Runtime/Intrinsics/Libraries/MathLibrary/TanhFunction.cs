using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class TanhFunction()
    : IntrinsicFunction(
        "tanh",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Tanh(Convert.ToDouble(args.First()));
}