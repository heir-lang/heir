using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Return(Token keyword, Expression expression) : Statement
{
    public Token Keyword { get; } = keyword;
    public Expression Expression { get; } = expression;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitReturnStatement(this);
    public override List<Token> GetTokens() => [Keyword, ..Expression.GetTokens()];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Return(");
        Expression.Display(indent + 1);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}