using Heir.Syntax;

namespace Heir.AST
{
    public class BinaryOp(SyntaxNode left, Token op, SyntaxNode right) : Expression
    {
        public SyntaxNode Left { get; } = left;
        public Token Operator { get; } = op;
        public SyntaxNode Right { get; } = right;

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BinaryOp(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Left ->");
            Left.Display(indent + 2);
            Console.WriteLine(",");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Right ->");
            Right.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
