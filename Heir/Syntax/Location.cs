namespace Heir.Syntax
{
    public class Location(string fileName, int line, int column, int position)
    {
        public string FileName { get; } = fileName;
        public int Line { get; } = line;
        public int Column { get; } = column;
        public int Position { get; } = position;

        public override string ToString() => $"{FileName}:{Line}:{Column}";
    }
}
