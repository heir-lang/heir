namespace Heir.Syntax;

public sealed class Location(string fileName, int line, int column, int position)
{
    public static readonly Location Empty = new("anonymous", 0, 0, 0);
    public static readonly Location Intrinsic = new("intrinsic", 0, 0, 0);
        
    public string FileName { get; } = fileName;
    public int Line { get; } = line;
    public int Column { get; } = column;
    public int Position { get; } = position;
        
    public bool Equals(Location other) =>
        FileName == other.FileName &&
        Line == other.Line &&
        Column == other.Column &&
        Position == other.Position;

    public override string ToString() => $"{FileName}:{Line}:{Column}";
}