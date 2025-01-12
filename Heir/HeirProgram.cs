namespace Heir;

public sealed class HeirProgram
{
    public HashSet<SourceFile> SourceFiles { get; } = [];
    
    public void LoadFile(SourceFile sourceFile) => SourceFiles.Add(sourceFile);
    public void LoadFiles(HashSet<SourceFile> sourceFiles)
    {
        foreach (var sourceFile in sourceFiles)
            LoadFile(sourceFile);
    }

    public object? Evaluate() =>
        SourceFiles
            .Select(sourceFile => sourceFile.Evaluate().Item1)
            .FirstOrDefault(result => result != null);
}