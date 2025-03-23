using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class CeilFunction()
    : IntrinsicFunction(
        "ceil",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Int
    )
{
    public override BaseDelegate Invoke { get; } = args => Convert.ToInt32(Math.Ceiling(Convert.ToDouble(args.First())));
}