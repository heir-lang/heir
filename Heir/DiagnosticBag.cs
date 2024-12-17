using Heir.Syntax;
using System.Collections;

namespace Heir
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly HashSet<Diagnostic> _diagnostics = [];

        public void Warn(string code, string message, Token token)
        {
            Warn(code, message, token.StartLocation, token.EndLocation);
        }

        public void Warn(string code, string message, Location startLocation, Location? endLocation = null)
        {
            var diagnostic = new Diagnostic(code, message, startLocation, endLocation ?? startLocation, DiagnosticLevel.Warn);
            _diagnostics.Add(diagnostic);
        }

        public void Error(string code, string message, Token token)
        {
            Error(code, message, token.StartLocation, token.EndLocation);
        }

        public void Error(string code, string message, Location startLocation, Location? endLocation = null)
        {
            var diagnostic = new Diagnostic(code, message, startLocation, endLocation ?? startLocation, DiagnosticLevel.Error);
            _diagnostics.Add(diagnostic);
        }

        public bool HasErrors()
        {
            return _diagnostics.Any(diagnostic => diagnostic.Level == DiagnosticLevel.Error);
        }

        public IEnumerator<Diagnostic> GetEnumerator()
        {
            return _diagnostics.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _diagnostics.GetEnumerator();
        }

        public string ToString(bool colors) => string.Join('\n', _diagnostics.Select(diagnostic => diagnostic.ToString(colors)));
    }
}
