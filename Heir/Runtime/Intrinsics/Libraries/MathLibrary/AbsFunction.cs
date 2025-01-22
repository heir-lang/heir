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
        if (argument is long l)
            return Convert.ToInt64(Math.Abs(l));
        
        return Math.Abs(Convert.ToDouble(argument));
    };
}