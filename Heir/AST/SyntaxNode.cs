namespace Heir.AST
{
    public abstract class Statement : SyntaxNode
    {
    }

    public abstract class Expression : SyntaxNode
    {
    }

    public abstract class SyntaxNode
    {
        public abstract void Display(int indent = 0);
    }
}
