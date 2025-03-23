using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class RoundFunction()
    : IntrinsicFunction(
        "round",
        new()
        {
            { "n", IntrinsicTypes.Number },
            { "digits", BaseType.Nullable(PrimitiveType.Int) }
        },
        [],
        IntrinsicTypes.Number
    )
{
    public override BaseDelegate Invoke { get; } = args =>
    {
        var n = Convert.ToDouble(args.First());
        if (args.Last() is int digits)
            return Convert.ToDouble(Math.Round(n, digits));

        return Convert.ToInt32(Math.Round(n));
    };
}