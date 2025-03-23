using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class CoshFunction()
    : IntrinsicFunction(
        "cosh",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Cosh(Convert.ToDouble(args.First()));
}