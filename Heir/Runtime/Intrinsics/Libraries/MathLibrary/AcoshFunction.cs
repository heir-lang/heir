using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AcoshFunction()
    : IntrinsicFunction(
        "acosh",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Acosh(Convert.ToDouble(args.First()));
}