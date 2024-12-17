using Heir.Syntax;
using Spectre.Console;

namespace Heir
{
    public enum DiagnosticLevel : byte
    {
        Warn,
        Error
    }

    public sealed class Diagnostic(string code, string message, Location startLocation, Location endLocation, DiagnosticLevel level)
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public Location StartLocation { get; } = startLocation;
        public Location EndLocation { get; } = endLocation;
        public DiagnosticLevel Level { get; } = level;

        public string ToString(bool colors)
        {
            var levelDisplay = Level.ToString().ToLower();
            var codeDisplay = Code;
            var messageDisplay = Message;
            var locationDisplay = StartLocation.ToString();

            if (colors)
            {
                codeDisplay = $"[grey]{codeDisplay}[/]";
                messageDisplay = $"[lightskyblue1]{messageDisplay}[/]";
                locationDisplay = $"[silver]{locationDisplay}[/]";

                switch (Level)
                {
                    case DiagnosticLevel.Error:
                        levelDisplay = $"[bold red]{levelDisplay}[/]";
                        break;
                    case DiagnosticLevel.Warn:
                        levelDisplay = $"[bold yellow]{levelDisplay}[/]";
                        break;
                }
            }

            return $"{locationDisplay} - {levelDisplay} {codeDisplay}: {messageDisplay}";
        }
    }
}
