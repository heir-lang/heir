namespace Heir.AST
{
    public class SyntaxTree(List<SyntaxNode> statements) : Block(statements)
    {
        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitSyntaxTree(this);

        public override void Display(int indent = 0)
        {
            foreach (var statement in Statements)
                statement.Display(indent);
        }
    }
}
