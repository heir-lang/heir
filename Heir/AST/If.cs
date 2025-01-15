using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class If(Token keyword, Expression condition, Statement body, Statement? elseBranch) : Statement
{
    public Token Keyword { get; } = keyword;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
    public Statement? ElseBranch { get; } = elseBranch;
    
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitIfStatement(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        ..Condition.GetTokens(),
        ..Body.GetTokens(),
        ..ElseBranch == null ? [] : ElseBranch.GetTokens()
    ];

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}If(");
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
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}