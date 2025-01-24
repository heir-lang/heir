using Heir.Syntax;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Heir.AST.Abstract;
using Spectre.Console;

namespace Heir;

public sealed class DiagnosticBag(SourceFile sourceFile, IEnumerable<Diagnostic>? initialCollection = null)
    : HashSet<Diagnostic>(initialCollection ?? [])
{
    public bool HasErrors => this.Any(diagnostic => diagnostic.Level == DiagnosticLevel.Error);
    public bool HasWarnings => this.Any(diagnostic => diagnostic.Level == DiagnosticLevel.Warn);

    public void Warn(DiagnosticCode code, string message, SyntaxNode startNode, SyntaxNode endNode) =>
        Warn(code, message, startNode.GetFirstToken(), endNode.GetLastToken());

    public void Warn(DiagnosticCode code, string message, SyntaxNode node) =>
        Warn(code, message, node.GetFirstToken(), node.GetLastToken());

    public void Warn(DiagnosticCode code, string message, Token startToken, Token endToken) =>
        Warn(code, message, startToken.Span.Start, endToken.Span.End);

    public void Warn(DiagnosticCode code, string message, Token token) =>
        Warn(code, message, token.Span.Start, token.Span.End);

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
        Error(code, message, startToken.Span.Start, endToken.Span.End);

    public void Error(DiagnosticCode code, string message, Token? token) =>
        Error(code, message, token?.Span.Start, token?.Span.End);
    
    public void Error(DiagnosticCode code, string message, Location? startLocation, Location? endLocation)
    {
        var diagnostic = new Diagnostic(sourceFile, code, message, startLocation ?? Location.Empty, endLocation ?? startLocation ?? Location.Empty, DiagnosticLevel.Error);
        Add(diagnostic);
    }

    [DoesNotReturn]
    public void RuntimeError(DiagnosticCode code, string message, Token? token)
    {
        Error(code, message, token?.Span.Start, token?.Span.End);
        throw new Exception();
    }

    public void Write(bool colors = true, bool all = false, bool clear = true)
    {
        if (colors)
            AnsiConsole.MarkupLine(ToString(true, all));
        else
            Console.WriteLine(ToString(false, all));

        if (clear)
            Clear();
    }
    
    public string ToString(bool colors, bool all = false) => all
        ? string.Join('\n', this.Select(diagnostic => diagnostic.ToString(colors)))
        : this.First().ToString(colors);
}