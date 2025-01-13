using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Block(List<Statement> statements) : Statement
{
    public List<Statement> Statements { get; } = statements;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBlock(this);

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Block(");
        foreach (var statement in Statements)
            statement.Display(indent + 1);

        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }        

    public override List<Token> GetTokens() =>
        Statements
            .SelectMany(statement => statement.GetTokens())
            .ToList();
}