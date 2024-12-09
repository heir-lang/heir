using Heir.Syntax;

namespace Heir
{
    public class DiagnosticBag
    {
        private readonly HashSet<Diagnostic> _diagnostics = [];

        public void Warn(string code, string message, Location location)
        {
            var diagnostic = new Diagnostic(code, message, location, DiagnosticLevel.Warn);
            _diagnostics.Add(diagnostic);
        }

        public void Error(string code, string message, Location location)
        {
            var diagnostic = new Diagnostic(code, message, location, DiagnosticLevel.Error);
            _diagnostics.Add(diagnostic);
        }
    }
}
