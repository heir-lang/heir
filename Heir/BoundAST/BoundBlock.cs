using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundBlock : BoundStatement
{
    public override BaseType Type { get; }
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

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundBlock(this);
    
    public override List<Token> GetTokens() =>
        Statements
            .Select(statement => statement.GetTokens())
            .Aggregate((allTokens, statementTokens) => allTokens.Concat(statementTokens).ToList());
        
    private static bool ContainsReturn(BoundStatement stmt)
    {
        if (stmt is BoundFunctionDeclaration)
            return false;

        if (stmt is BoundBlock block)
            return block.Statements.Any(ContainsReturn);

        return stmt is BoundReturn;
    }

    private static List<BoundReturn> GetReturn(BoundStatement stmt)
    {
        return (stmt switch
        {
            BoundBlock block => block.Statements.SelectMany(GetReturn),
            BoundFunctionDeclaration functionDeclaration => functionDeclaration.Body.Statements.SelectMany(GetReturn),
            BoundReturn returnStatement => [returnStatement],
            _ => stmt.GetType()
                .GetProperties()
                .Select(prop => prop.GetValue(stmt))
                .OfType<BoundReturn>()
        }).ToList();
    }
}