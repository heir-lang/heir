using System.Text;

namespace Heir.Runtime.Values;

public sealed class ObjectValue(IEnumerable<KeyValuePair<object, object?>> pairs) : Dictionary<object, object?>(pairs), IValue
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
            result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
            result.Append('[');
            result.Append(Utility.Repr(property.Key, colors));
            result.Append("]: ");

            var newIndent = indent + 1;
            var valueString = property.Value is ObjectValue objectValue
                ? objectValue.ToString(ref newIndent, colors)
                : Utility.Repr(property.Value, colors);
            
            result.Append(valueString);
            if (Keys.ToList().IndexOf(property.Key) != Count - 1)
                result.AppendLine(",");
        }

        if (Count > 0)
        {
            result.AppendLine();
            indent--;
        }
            
        result.Append('}');
        return result.ToString();
    }
}