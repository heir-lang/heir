namespace Heir.Runtime;

public sealed class Scope(Scope? enclosing = null)
{
    private static int _cumulativeID;
    
    public int ID { get; } = _cumulativeID++;
    public Scope? Enclosing { get; } = enclosing;

    private readonly Dictionary<string, bool> _defined = [];
    private readonly Dictionary<string, object?> _values = [];

    public void AssignAt(string name, object? value, uint distance)
    {
        var scope = Ancestor(distance);
        if (scope == null) return;
            
        scope._values[name] = value;
        scope._defined[name] = value != null;
    }

    public void Assign(string name, object? value)
    {
        if (_values.ContainsKey(name))
        {
            _values[name] = value;
            _defined[name] = value != null;
            return;
        }

        Enclosing?.Assign(name, value);
    }

    public void Define(string name, object? value)
    {
        _values[name] = value;
        _defined[name] = value != null;
    }

    public bool IsDeclared(string name) => _defined.TryGetValue(name, out _) || (Enclosing?.IsDeclared(name) ?? false);
    public bool IsDefined(string name)
    {
        if (_defined.TryGetValue(name, out var isDefined))
            return isDefined;
            
        return Enclosing?.IsDefined(name) ?? false;
    }

    public T? Lookup<T>(string name) => (T?)Lookup(name);
    public object? Lookup(string name)
    {
        var foundValue = _values.TryGetValue(name, out var value);
        return foundValue && value != null
            ? value
            : Enclosing?.Lookup(name) ?? value;
    }

    public T? LookupAt<T>(string name, uint distance) => (T?)LookupAt(name, distance);
    public object? LookupAt(string name, uint distance)
    {
        var values = Ancestor(distance)?._values;
        if (values != null && values.TryGetValue(name, out var value))
            return value;

        return null;
    }

    public Scope? Ancestor(uint distance)
    {
        var env = this;
        for (var i = 0; i < distance; i++)
        {
            if (env == null) break;
            env = env.Enclosing;
        }

        return env;
    }
}