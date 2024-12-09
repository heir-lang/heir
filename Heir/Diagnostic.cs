using Heir.Syntax;

namespace Heir
{
    public enum DiagnosticLevel : byte
    {
        Warn,
        Error
    }

    public class Diagnostic(string code, string message, Location startLocation, Location endLocation, DiagnosticLevel level)
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public Location StartLocation { get; } = startLocation;
        public Location EndLocation { get; } = endLocation;
        public DiagnosticLevel Level { get; } = level;
    }
}
