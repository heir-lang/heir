using Heir.Runtime.Values;
using Heir.Types;

namespace Heir.Runtime.Intrinsics.Libraries.MathLibrary;

public class MathLibrary() : IntrinsicLibrary("math", InterfaceType.Readonly("math", _memberTypes))
{
    private static readonly Dictionary<string, BaseType> _memberTypes;
    private static readonly Dictionary<object, object?> _members;
    private static readonly List<IntrinsicFunction> _functions =
    [
        new AbsFunction(),
        new FloorFunction(),
        new CeilFunction(),
        new RoundFunction(),
        new SinFunction(),
        new CosFunction(),
        new TanFunction(),
        new SinhFunction(),
        new CoshFunction(),
        new TanhFunction(),
        new AsinFunction(),
        new AcosFunction(),
        new AtanFunction(),
        new AsinhFunction(),
        new AcoshFunction(),
        new AtanhFunction(),
        new InRadiansFunction(),
        new InDegreesFunction()
    ];

    static MathLibrary()
    {
        var memberTypes = new Dictionary<string, BaseType>
        {
            { "pi", PrimitiveType.Float },
            { "e", PrimitiveType.Float },
            { "tau", PrimitiveType.Float },
            { "inf", PrimitiveType.Float }
        };

        var members = new Dictionary<object, object?>
        {
            { "pi", Math.PI },
            { "e", Math.E },
            { "tau", Math.Tau },
            { "inf", double.PositiveInfinity }
        };

        foreach (var function in _functions)
        {
            memberTypes.Add(function.Name, function.Type);
            members.Add(function.Name, function);
        }

        _memberTypes = memberTypes;
        _members = members;
    }

    public override object? Value { get; } = new ObjectValue(_members);
}