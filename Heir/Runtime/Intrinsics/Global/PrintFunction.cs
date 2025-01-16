using Heir.Types;
using Spectre.Console;

namespace Heir.Runtime.Intrinsics.Global;

public delegate void PrintIntrinsic(object? value);

public class PrintFunction()
    : IntrinsicFunction<PrintIntrinsic>(
        "print",
        new()
        {
            { "value", IntrinsicTypes.Any }
        },
        PrimitiveType.None,
        true
    )
{
    public override PrintIntrinsic Call { get; } = value =>
    {
        if (value is string)
            Console.WriteLine(value);
        else
            AnsiConsole.MarkupLine(Utility.Repr(value, true));
    };
}