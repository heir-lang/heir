using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class MathLibrary() : IntrinsicLibrary("math", _type)
{
    private static readonly AbsFunction _absFunction = new();
    private static readonly InterfaceType _type = InterfaceType.Readonly("math", new()
    {
        { "pi", PrimitiveType.Float },
        { "e", PrimitiveType.Float },
        { "tau", PrimitiveType.Float },
        { "inf", PrimitiveType.Float },
        { "abs", _absFunction.Type }
    });

    public override object? Value { get; } = new ObjectValue([
        new("pi", Math.PI),
        new("e", Math.E),
        new("tau", Math.Tau),
        new("inf", double.PositiveInfinity),
        new("abs", _absFunction.Value)
    ]);
}