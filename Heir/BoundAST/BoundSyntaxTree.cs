namespace Heir.BoundAST
{
    public class BoundSyntaxTree(List<BoundSyntaxNode> statements, DiagnosticBag diagnostics) : BoundBlock(statements)
    {
        public DiagnosticBag Diagnostics { get; } = diagnostics;

        public override void Display(int indent = 0)
        {
            foreach (var statement in Statements)
                statement.Display(indent);
        }
    }
}
