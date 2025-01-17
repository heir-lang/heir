using Heir.Types;
using Spectre.Console;

namespace Heir.Runtime.Intrinsics.Global;

public class PrintFunction()
    : IntrinsicFunction(
        "print",
        new()
        {
            { "value", IntrinsicTypes.Any }
        },
        PrimitiveType.None,
        true
    )
{
    public override BaseDelegate Invoke { get; } = values =>
    {
        foreach (var value in values ?? [])
        {
            if (value is string)
                Console.WriteLine(value);
            else
                AnsiConsole.MarkupLine(Utility.Repr(value, true));
        }

        return null;
    };
}