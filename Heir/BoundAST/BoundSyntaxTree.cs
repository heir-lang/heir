using Heir.BoundAST.Abstract;
using Heir.Diagnostics;

namespace Heir.BoundAST;

public sealed class BoundSyntaxTree(List<BoundStatement> statements, DiagnosticBag diagnostics) : BoundBlock(statements)
{
    public DiagnosticBag Diagnostics { get; } = diagnostics;
}