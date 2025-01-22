using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AbsFunction()
    : IntrinsicFunction(
        "abs",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        IntrinsicTypes.Number
    )
{
    public override BaseDelegate Invoke { get; } = args =>
    {
        var argument = args.First();
        return argument switch
        {
            double n => Math.Abs(n),
            long n => Math.Abs(n),
            _ => Math.Abs(Convert.ToDouble(argument))
        };
    };
}