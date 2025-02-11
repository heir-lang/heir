using Heir.Types;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class Q_RsqrtFunction()
    : IntrinsicFunction(
        "q_rsqrt",
        new()
        {
            { "n", IntrinsicTypes.Number },
            { "iterations", BaseType.Nullable(PrimitiveType.Int) }
        },
        PrimitiveType.Float
    )
{
    public override unsafe BaseDelegate Invoke { get; } = args =>
    {
        var number = Convert.ToSingle(args.First());
        var iterations = Convert.ToInt32(args.LastOrDefault() ?? 1);
        const float threeHalves = 1.5f;
        
        var x2 = number * 0.5f;
        var y = number;
        var i = *(long*)&y;              // evil floating point bit level hacking
        i = 0x5f3759df - ( i >> 1 );          // what the fuck?
        y = *(float*)&i;
        
        for (var j = 0; j < iterations; j++)
            y *= threeHalves - ( x2 * y * y );
        
        return y;
    };
}