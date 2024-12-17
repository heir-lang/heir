namespace Heir
{
    public sealed class Scope(DiagnosticBag diagnostics, Scope? enclosing = null)
    {
        public Scope? Enclosing { get; } = enclosing;

        private readonly DiagnosticBag _diagnostics = diagnostics;
        private readonly Dictionary<string, bool> _defined = [];
        private readonly Dictionary<string, object?> _values = [];

        public void AssignAt(string name, object? value, uint distance)
        {
            var scope = Ancestor(distance);
            if (scope != null)
            {
                scope._values[name] = value;
                scope._defined[name] = value != null;
            }
        }

        public void Assign(string name, object? value)
        {
            if (_values.ContainsKey(name))
            {
                _values[name] = value;
                _defined[name] = value != null;
                return;
            }

            if (Enclosing == null) return;
            Enclosing.Assign(name, value);
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
            if (_values.TryGetValue(name, out var value))
                return value;

            if (Enclosing != null)
                return Enclosing.Lookup(name);

            return null;
        }

        public T? LookupAt<T>(string name, uint distance) => (T?)LookupAt(name, distance);
        public object? LookupAt(string name, uint distance)
        {
            var values = Ancestor(distance)?._values;
            if (values != null && values.TryGetValue(name, out var value))
                return value;

            return default;
        }

        public Scope? Ancestor(uint distance)
        {
            var env = this;
            for (var i = 0; i < distance; i++)
                env = env.Enclosing!;

            return env;
        }
    }
}
