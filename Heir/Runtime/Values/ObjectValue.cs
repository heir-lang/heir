using System.Text;

namespace Heir.Runtime.Values;

public sealed class ObjectValue(IEnumerable<KeyValuePair<object, object?>> pairs)
    : Dictionary<object, object?>(pairs), IValue
{
    public string ToString(ref int indent, bool colors = false)
    {
        var result = new StringBuilder("{");
        if (Count > 0)
        {
            result.AppendLine();
            indent++;
        }

        foreach (var property in this)
        {
            result.Append(string.Join("", Enumerable.Repeat("  ", indent)))
                .Append("[[")
                .Append(Utility.Repr(property.Key, colors))
                .Append("]]: ");

            var newIndent = indent + 1;
            var valueString = property.Value switch
            {
                ArrayValue arrayValue => arrayValue.ToString(ref newIndent, colors),
                ObjectValue objectValue => objectValue.ToString(ref newIndent, colors),
                _ => Utility.Repr(property.Value, colors)
            };

            result.Append(valueString);
            if (Keys.ToList().IndexOf(property.Key) != Count - 1)
                result.AppendLine(",");
        }

        if (Count > 0)
        {
            result.AppendLine();
            indent--;
        }

        if (indent > 0)
            result.Append(string.Join("", Enumerable.Repeat("  ", indent - 1)));
        
        return result.Append('}').ToString();
    }
}