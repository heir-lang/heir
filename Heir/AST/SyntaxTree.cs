using Heir.AST.Abstract;

namespace Heir.AST;

public class SyntaxTree(List<Statement> statements, DiagnosticBag diagnostics) : Block(statements)
{
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitSyntaxTree(this);

    public DiagnosticBag Diagnostics { get; } = diagnostics;
}