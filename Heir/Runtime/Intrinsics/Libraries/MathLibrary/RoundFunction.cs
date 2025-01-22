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
        IntrinsicTypes.Number
    )
{
    public override BaseDelegate Invoke { get; } = args =>
    {
        var n = Convert.ToDouble(args.First());
        if (args.Last() is long digits)
            return Convert.ToDouble(Math.Round(n, (int)digits));

        return Convert.ToInt64(Math.Round(n));
    };
}