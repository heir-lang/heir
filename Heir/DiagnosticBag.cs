using Heir.Syntax;
using System.Collections;
using Heir.AST.Abstract;

namespace Heir;

public sealed class DiagnosticBag(SourceFile sourceFile) : HashSet<Diagnostic>
{
    public bool HasErrors => this.Any(diagnostic => diagnostic.Level == DiagnosticLevel.Error);
    public bool HasWarnings => this.Any(diagnostic => diagnostic.Level == DiagnosticLevel.Warn);

    public void Warn(DiagnosticCode code, string message, SyntaxNode startNode, SyntaxNode endNode) =>
        Warn(code, message, startNode.GetFirstToken(), endNode.GetLastToken());

    public void Warn(DiagnosticCode code, string message, SyntaxNode node) =>
        Warn(code, message, node.GetFirstToken(), node.GetLastToken());

    public void Warn(DiagnosticCode code, string message, Token startToken, Token endToken) =>
        Warn(code, message, startToken.StartLocation, endToken.EndLocation);

    public void Warn(DiagnosticCode code, string message, Token token) =>
        Warn(code, message, token.StartLocation, token.EndLocation);

    public void Warn(DiagnosticCode code, string message, Location startLocation, Location? endLocation = null)
    {
        var diagnostic = new Diagnostic(sourceFile, code, message, startLocation, endLocation ?? startLocation, DiagnosticLevel.Warn);
        Add(diagnostic);
    }

    public void Error(DiagnosticCode code, string message, SyntaxNode startNode, SyntaxNode endNode) =>
        Error(code, message, startNode.GetFirstToken(), endNode.GetLastToken());

    public void Error(DiagnosticCode code, string message, SyntaxNode node) =>
        Error(code, message, node.GetFirstToken(), node.GetLastToken());

    public void Error(DiagnosticCode code, string message, Token startToken, Token endToken) =>
        Error(code, message, startToken.StartLocation, endToken.EndLocation);

    public void Error(DiagnosticCode code, string message, Token token) =>
        Error(code, message, token.StartLocation, token.EndLocation);

    public void Error(DiagnosticCode code, string message, Location startLocation, Location? endLocation = null)
    {
        var diagnostic = new Diagnostic(sourceFile, code, message, startLocation, endLocation ?? startLocation, DiagnosticLevel.Error);
        Add(diagnostic);
    }
    
    public string ToString(bool colors) => string.Join('\n', this.Select(diagnostic => diagnostic.ToString(colors)));
}