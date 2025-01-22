using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries;

public class MathLibrary() : IntrinsicLibrary("math", _type)
{
    private static readonly InterfaceType _type = InterfaceType.Readonly("math", new()
    {
        { "pi", PrimitiveType.Float }
    });

    public override object? Value { get; } = new ObjectValue([
        new("pi", Math.PI),
    ]);
}