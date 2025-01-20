using Heir.Runtime.Values;

namespace Heir;

public static class Utility
{
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