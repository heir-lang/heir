using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Block(List<Statement> statements) : Statement
{
    public List<Statement> Statements { get; } = statements;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBlock(this);

    public override List<Token> GetTokens() =>
        Statements
            .SelectMany(statement => statement.GetTokens())
            .ToList();
}