using Heir.Syntax;

namespace Heir
{
    public enum DiagnosticCode : byte
    {
        HDEV,
        H001,
        H001B,
        H001C,
        H001D,
        H002,
        H002B,
        H003,
        H004,
        H004B,
        H004C,
        H004D,
        H005,
        H006,
        H006B,
        H006C,
        H007,
        H008,
        H009,
        H010,
        H011,
        H012
    }

    public enum DiagnosticLevel : byte
    {
        Warn,
        Error
    }

    public sealed class Diagnostic(DiagnosticCode code, string message, Location startLocation, Location endLocation, DiagnosticLevel level)
    {
        public DiagnosticCode Code { get; } = code;
        public string Message { get; } = message;
        public Location StartLocation { get; } = startLocation;
        public Location EndLocation { get; } = endLocation;
        public DiagnosticLevel Level { get; } = level;

        public string ToString(bool colors)
        {
            var levelDisplay = Level.ToString().ToLower();
            var codeDisplay = Code.ToString();
            var messageDisplay = Message;
            var locationDisplay = StartLocation.ToString();

            if (colors)
            {
                codeDisplay = $"[grey]{codeDisplay}[/]";
                messageDisplay = $"[silver]{messageDisplay}[/]";
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
