using Heir.Syntax;

namespace Heir.AST
{
    public class UnaryOp(SyntaxNode operand, Token op) : Expression
    {
        public SyntaxNode Operand { get; } = operand;
        public Token Operator { get; } = op;

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Unary(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operand ->");
            Operand.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
