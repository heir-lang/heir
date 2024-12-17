namespace Heir.AST
{
    public class SyntaxTree(List<Statement> statements, DiagnosticBag diagnostics) : Block(statements)
    {
        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitSyntaxTree(this);

        public DiagnosticBag Diagnostics { get; } = diagnostics;

        public override void Display(int indent = 0)
        {
            foreach (var statement in Statements)
            {
                statement.Display(indent);
                Console.WriteLine();
            }
        }
    }
}
