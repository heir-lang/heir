using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundBlock : BoundStatement
{
    public override BaseType? Type { get; }
    public List<BoundStatement> Statements { get; }

    public BoundBlock(List<BoundStatement> statements)
    {
        Statements = statements;
            
        var returnStatements = Statements
            .Where(ContainsReturn)
            .SelectMany(GetReturn)
            .Select(returnStmt => returnStmt.Type)
            .ToList();
        
        Type = returnStatements.Count == 0
            ? PrimitiveType.None
            : returnStatements.Aggregate((finalType, currentType) =>
            {
                if (finalType is UnionType finalUnion)
                {
                    if (currentType is UnionType currentUnion)
                        return new UnionType([..finalUnion.Types, ..currentUnion.Types]);
                    
                    return new UnionType([..finalUnion.Types, currentType]);
                }

                if (currentType is UnionType union)
                    return new UnionType([finalType, ..union.Types]);
                
                return new UnionType([finalType, currentType]);
            });
    }

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundBlock(this);

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
        
    private static bool ContainsReturn(BoundStatement stmt)
    {
        // if (stmt is BoundFunctionDeclaration)
        //     return false;

        if (stmt is BoundBlock block)
            return block.Statements.Any(ContainsReturn);

        return stmt is BoundReturn;
    }

    private static IEnumerable<BoundReturn> GetReturn(BoundStatement stmt)
    {
        return stmt switch
        {
            BoundBlock block => block.Statements.SelectMany(GetReturn),
            BoundReturn returnStatement => [returnStatement],
            // BoundFunctionDeclaration functionDeclaration => functionDeclaration.Body,
            _ => stmt.GetType()
                .GetProperties()
                .Select(prop => prop.GetValue(stmt))
                .OfType<BoundReturn>()
        };
    }
}