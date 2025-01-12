using System.Text;

namespace Heir.Runtime;

public sealed class ObjectValue(IEnumerable<KeyValuePair<object, object?>> pairs) : Dictionary<object, object?>(pairs)
{
    public string ToString(ref int indent)
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
            result.Append(property.Key.ToString());
            result.Append("]: ");

            var newIndent = indent + 1;
            var valueString = property.Value is ObjectValue objectValue
                ? objectValue.ToString(ref newIndent)
                : property.Value?.ToString() ?? "none";
            
            result.Append(valueString); // TODO:repr function thing again
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