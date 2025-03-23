using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class AbsFunction()
    : IntrinsicFunction(
        "abs",
        new()
        {
            { "n", IntrinsicTypes.Number }
        },
        [],
        IntrinsicTypes.Number
    )
{
    public override BaseDelegate Invoke { get; } = args =>
    {
        var argument = args.First();
        if (argument is int i)
            return Convert.ToInt32(Math.Abs(i));
        
        return Math.Abs(Convert.ToDouble(argument));
    };
}