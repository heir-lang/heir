using Heir.Types;

namespace Heir.Runtime.Intrinsics.Global;

public class NowFunction()
    : IntrinsicFunction(
        "now",
        [],
        [],
        PrimitiveType.Float,
        true
    )
{
    public override BaseDelegate Invoke { get; } = _ => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
}