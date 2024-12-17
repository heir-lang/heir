using Heir.Syntax;

namespace Heir
{
    public interface VariableOptions
    {
        bool IsMutable { get; }
    }

    public class Scope(DiagnosticBag diagnostics, Scope? enclosing)
    {
        public Scope? Enclosing { get; } = enclosing;

        private readonly DiagnosticBag _diagnostics = diagnostics;
        private readonly Dictionary<string, bool> _defined = [];
        private readonly Dictionary<string, object?> _values = [];
        private readonly Dictionary<string, VariableOptions> _options = [];

        public void AssignAt(Token name, object? value, uint distance)
        {
            var scope = Ancestor(distance);
            scope?.CheckImmutability(name);
            if (scope != null)
            {
                scope._values[name.Text] = value;
                scope._defined[name.Text] = value != null;
            }
        }

        public void Assign(Token name, object? value)
        {
            if (_values.ContainsKey(name.Text))
            {
                CheckImmutability(name);
                _values[name.Text] = value;
                _defined[name.Text] = value != null;
                return;
            }

            if (Enclosing == null) return;
            Enclosing.Assign(name, value);
        }

        public void Define(Token name, object? value, VariableOptions options)
        {
            _values[name.Text] = value;
            _options[name.Text] = options;
            _defined[name.Text] = value != null;
        }

        public T? Get<T>(Token name)
        {
            if (_values.TryGetValue(name.Text, out var value))
                return (T?)value;

            if (Enclosing != null)
                return Enclosing.Get<T>(name);

            return default;
        }

        public T? GetAt<T>(Token name, uint distance)
        {
            var values = Ancestor(distance)?._values;
            if (values != null && values.TryGetValue(name.Text, out var value))
                return (T?)value;

            return default;
        }

        public Scope? Ancestor(uint distance)
        {
            var env = this;
            for (var i = 0; i < distance; i++)
                env = env.Enclosing!;

            return env;
        }

        private void CheckImmutability(Token name)
        {
            var isMutable = _options[name.Text].IsMutable;
            var isDefined = _defined[name.Text];
            if (!isMutable && isDefined)
                _diagnostics.Error("H015", $"'{name}' is not defined in this scope", name   .Token);
        }
    }
}
