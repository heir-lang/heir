using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class SqrtFunction()
    : IntrinsicFunction(
        "sqrt",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Sqrt(Convert.ToDouble(args.First()));
}