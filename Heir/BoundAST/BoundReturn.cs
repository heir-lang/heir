using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundReturn(Token keyword, BoundExpression expression) : BoundStatement
{
    public override BaseType Type => Expression.Type;

    public Token Keyword { get; } = keyword;
    public BoundExpression Expression { get; } = expression;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundReturnStatement(this);
    public override List<Token> GetTokens() => [];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundReturn(");
        Expression.Display(indent + 1);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}