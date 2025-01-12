using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class UnaryOp(Expression operand, Token op) : Expression
{
    public Expression Operand { get; } = operand;
    public Token Operator { get; } = op;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnaryOpExpression(this);
    public override List<Token> GetTokens() => Operand.GetTokens().Append(Operator).ToList();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Unary(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operand ->");
        Operand.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}