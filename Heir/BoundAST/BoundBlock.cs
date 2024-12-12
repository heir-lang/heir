using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundBlock(List<BoundSyntaxNode> statements) : BoundStatement
    {
        public override BaseType? Type => null; // temp
        public List<BoundSyntaxNode> Statements { get; } = statements;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundBlockStatement(this);

        public override void Display(int indent = 0)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundBlock(");
            foreach (var statement in Statements)
                statement.Display(indent + 1);

            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }

        public override List<Token> GetTokens() =>
            Statements
            .Select(statement => statement.GetTokens())
            .Aggregate((allTokens, statementTokens) => allTokens.Concat(statementTokens).ToList());
    }
}
