using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class MemberAccess(Expression expression, IdentifierName name) : Expression
{
    public Expression Expression { get; } = expression;
    public IdentifierName Name { get; } = name;
    
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitMemberAccessExpression(this);
    public override List<Token> GetTokens() => [..Expression.GetTokens(), ..Name.GetTokens()];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}MemberAccess(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Expression ->");
        Expression.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Name ->");
        Name.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}