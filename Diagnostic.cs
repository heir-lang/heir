using Heir.Syntax;

namespace Heir
{
    public enum DiagnosticLevel : byte
    {
        Warn,
        Error
    }

    public class Diagnostic(string code, string message, Location location, DiagnosticLevel level)
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public Location Location { get; } = location;
        public DiagnosticLevel Level { get; } = level;
    }
}
