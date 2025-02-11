using System.Text.RegularExpressions;
using Heir.Runtime.Values;

namespace Heir;

public static class Utility
{
    public static unsafe float Q_rsqrt(float number)
    {
        const float threeHalves = 1.5f;

        var x2 = number * 0.5f;
        var y = number;
        var i = *(long*)&y;                 // evil floating point bit level hacking
        i = 0x5f3759df - ( i >> 1 );   // what the fuck?
        y = *(float*)&i;
        y *= threeHalves - ( x2 * y * y );

        return y;
    }
    
    public static string EscapeTabsAndNewlines(string text) =>
        Regex.Replace(text, @"\s", match =>
            match.Value switch
            {
                "\r" => "\\r",
                "\n" => "\\n",
                "\t" => "\\t",
                _ => match.Value // fallback, even though \s covers most whitespace
            }
        );
    
    public static bool ContainsSequence<T>(IReadOnlyList<T> list, IReadOnlyList<T> sequence)
    {
        if (sequence.Count > list.Count)
            return false;

        for (var i = 0; i <= list.Count - sequence.Count; i++)
        {
            var match = !sequence.Where((t, j) => !(list[i + j]?.Equals(t) ?? false)).Any();
            if (match)
                return true;
        }

        return false;
    }
    
    public static bool DictionariesAreEqual<TKey, TValue>(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
        where TKey : notnull
    {
        if (a.Count != b.Count)
            return false;

        foreach (var pair in a)
            if (!b.TryGetValue(pair.Key, out var value) || !EqualityComparer<TValue>.Default.Equals(pair.Value, value))
                return false;

        return true;
    }
    
    public static string Repr(object? value, bool colors = false)
    {
        switch (value)
        {
            case ObjectValue objectValue:
            {
                var indent = 0;
                return objectValue.ToString(ref indent, colors);
            }
            case FunctionValue function: 
                return (colors ? "[lightyellow3]" : "") + function + ColorReset(colors);
            case bool:
                return (colors ? "[violet]" : "") + value.ToString()?.ToLower() + ColorReset(colors);
            case char:
                return (colors ? "[springgreen3_1]" : "") + "'" + value + "'" + ColorReset(colors);
            case string:
                return (colors ? "[springgreen3_1]" : "") + '"' + value + '"' + ColorReset(colors);
            case long or ulong or int or uint or short or ushort or byte or sbyte or double or float or decimal:
                return (colors ? "[orange3]" : "") + value + ColorReset(colors);
            case null:
                return (colors ? "[bold deepskyblue2]" : "") + "none" + ColorReset(colors);
        }

        return value.ToString()!;
    }
    
    private static string ColorReset(bool colors) => colors ? "[/]" : "";
}