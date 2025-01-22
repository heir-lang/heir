using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class CosFunction()
    : IntrinsicFunction(
        "cos",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Cos(Convert.ToDouble(args.First()));
}