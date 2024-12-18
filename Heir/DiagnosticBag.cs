using Heir.AST;
using Heir.Syntax;
using System.Collections;

namespace Heir
{
    public sealed class DiagnosticBag(SourceFile sourceFile) : IEnumerable<Diagnostic>
    {
        public bool HasErrors
        {
            get => _diagnostics.Any(diagnostic => diagnostic.Level == DiagnosticLevel.Error);
        }

        private readonly SourceFile _sourceFile = sourceFile;
        private readonly HashSet<Diagnostic> _diagnostics = [];

        public void Warn(DiagnosticCode code, string message, SyntaxNode startNode, SyntaxNode endNode)
        {
            Warn(code, message, startNode.GetFirstToken(), endNode.GetLastToken());
        }

        public void Warn(DiagnosticCode code, string message, SyntaxNode node)
        {
            Warn(code, message, node.GetFirstToken(), node.GetLastToken());
        }

        public void Warn(DiagnosticCode code, string message, Token startToken, Token endToken)
        {
            Warn(code, message, startToken.StartLocation, endToken.EndLocation);
        }

        public void Warn(DiagnosticCode code, string message, Token token)
        {
            Warn(code, message, token.StartLocation, token.EndLocation);
        }

        public void Warn(DiagnosticCode code, string message, Location startLocation, Location? endLocation = null)
        {
            var diagnostic = new Diagnostic(_sourceFile, code, message, startLocation, endLocation ?? startLocation, DiagnosticLevel.Warn);
            _diagnostics.Add(diagnostic);
        }

        public void Error(DiagnosticCode code, string message, SyntaxNode startNode, SyntaxNode endNode)
        {
            Error(code, message, startNode.GetFirstToken(), endNode.GetLastToken());
        }

        public void Error(DiagnosticCode code, string message, SyntaxNode node)
        {
            Error(code, message, node.GetFirstToken(), node.GetLastToken());
        }

        public void Error(DiagnosticCode code, string message, Token startToken, Token endToken)
        {
            Error(code, message, startToken.StartLocation, endToken.EndLocation);
        }

        public void Error(DiagnosticCode code, string message, Token token)
        {
            Error(code, message, token.StartLocation, token.EndLocation);
        }

        public void Error(DiagnosticCode code, string message, Location startLocation, Location? endLocation = null)
        {
            var diagnostic = new Diagnostic(_sourceFile, code, message, startLocation, endLocation ?? startLocation, DiagnosticLevel.Error);
            _diagnostics.Add(diagnostic);
        }

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _diagnostics.GetEnumerator();
        public string ToString(bool colors) => string.Join('\n', _diagnostics.Select(diagnostic => diagnostic.ToString(colors)));
    }
}
