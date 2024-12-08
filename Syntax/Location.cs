namespace Heir.Syntax
{
    public class Location(string fileName, uint line, uint column)
    {
        string FileName { get; } = fileName;
        uint Line { get; } = line;
        uint Column { get; } = column;
    }
}
