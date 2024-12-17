namespace Heir
{
    public sealed class SourceFile(string source, string? path)
    {
        public string Path { get; } = path ?? "<anonymous>";
        public string Source { get; } = source;
    }
}
