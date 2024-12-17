namespace Heir;

public sealed class HeirProgram
{
    public HashSet<SourceFile> SourceFiles { get; private set; } = [];

    private readonly List<DiagnosticBag> _diagnostics = [];

    public void LoadFile(SourceFile sourceFile) => SourceFiles.Add(sourceFile);
    public void LoadFiles(HashSet<SourceFile> sourceFiles)
    {
        foreach (var sourceFile in sourceFiles)
            LoadFile(sourceFile);
    }

    public object? Evaluate() =>
        SourceFiles
        .Select(sourceFile => sourceFile.Evaluate())
        .Where(result => result != null)
        .FirstOrDefault();
}