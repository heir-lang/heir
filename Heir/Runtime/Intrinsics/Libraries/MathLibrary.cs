using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries;

public class MathLibrary() : IntrinsicLibrary("math", _type)
{
    private static readonly InterfaceType _type = InterfaceType.Readonly("math", new()
    {
        { "pi", PrimitiveType.Float },
        { "e", PrimitiveType.Float },
        { "tau", PrimitiveType.Float },
        { "inf", PrimitiveType.Float }
    });

    public override object? Value { get; } = new ObjectValue([
        new("pi", Math.PI),
        new("e", Math.E),
        new("tau", Math.Tau),
        new("inf", double.PositiveInfinity)
    ]);
}