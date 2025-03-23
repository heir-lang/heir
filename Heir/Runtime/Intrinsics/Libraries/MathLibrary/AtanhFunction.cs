using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AtanhFunction()
    : IntrinsicFunction(
        "atanh",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        PrimitiveType.Float
    )
{
    public override BaseDelegate Invoke { get; } = args => Math.Atanh(Convert.ToDouble(args.First()));
}