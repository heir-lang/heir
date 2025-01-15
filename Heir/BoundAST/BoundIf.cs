using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundIf(Token keyword, BoundExpression condition, BoundStatement body, BoundStatement? elseBranch)
    : BoundStatement
{
    public override BaseType? Type => null;
    
    public Token Keyword { get; } = keyword;
    public BoundExpression Condition { get; } = condition;
    public BoundStatement Body { get; } = body;
    public BoundStatement? ElseBranch { get; } = elseBranch;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundIfStatement(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        ..Condition.GetTokens(),
        ..Body.GetTokens(),
        ..ElseBranch == null ? [] : ElseBranch.GetTokens()
    ];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundIf(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Condition ->");
        Condition.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Body ->");
        Body.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}ElseBranch ->{(ElseBranch == null ? " (none)" : "\n")}");
        ElseBranch?.Display(indent + 2);
        Console.WriteLine(',');
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}