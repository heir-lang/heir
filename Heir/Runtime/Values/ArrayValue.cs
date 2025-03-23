using System.Collections;
using System.Text;

namespace Heir.Runtime.Values;

public sealed class ArrayValue(IEnumerable<object?> elements) : IEnumerable<object?>, IValue
{
    public IEnumerable<object?> Elements { get; private set; } = elements;

    public object? this[int index]
    {
        get => Elements.ElementAtOrDefault(index);
        set => Elements = Elements.Select((v, i) => i == index ? value : v);
    }

    public string ToString(ref int indent, bool colors = false)
    {
        var result = new StringBuilder(colors ? "[[" : "[");
        var count = Elements.Count();
        const int elementCountForNewlines = 5;
        if (count > elementCountForNewlines)
        {
            result.AppendLine();
            indent++;
        }

        var i = 0;
        foreach (var element in this)
        {
            var newIndent = indent + 1;
            var valueString = element switch
            {
                ArrayValue arrayValue => arrayValue.ToString(ref newIndent, colors),
                ObjectValue objectValue => objectValue.ToString(ref newIndent, colors),
                _ => Utility.Repr(element, colors)
            };

            if (count > elementCountForNewlines)
                result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
                    
            result.Append(valueString);
            if (i++ != count - 1)
                result.Append(',').Append(count > elementCountForNewlines ? '\n' : ' ');
        }

        if (count > elementCountForNewlines)
        {
            result.AppendLine();
            indent--;
        }

        return result.Append(colors ? "]]" : "]").ToString();
    }

    public IEnumerator<object?> GetEnumerator() => Elements.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}