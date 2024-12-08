namespace Heir.Syntax
{
    public class Location(string fileName, int line, int column)
    {
        string FileName { get; } = fileName;
        int Line { get; } = line;
        int Column { get; } = column;

        public override string ToString()
        {
            return $"{FileName}:{Line}:{Column}";
        }
    }
}
