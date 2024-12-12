namespace Heir.BoundAST
{
    public class BoundSyntaxTree(List<BoundSyntaxNode> statements) : BoundBlock(statements)
    {
        public override void Display(int indent = 0)
        {
            foreach (var statement in Statements)
                statement.Display(indent);
        }
    }
}
