using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.AST;

public sealed class InterfaceDeclaration(Token keyword, Token identifier, List<InterfaceField> fields) : Statement
{
    public Token Keyword { get; } = keyword;
    public Token Identifier { get; } = identifier;
    public List<InterfaceField> Fields { get; } = fields;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitInterfaceDeclaration(this);

    public override List<Token> GetTokens() =>
    [
        Keyword,
        Identifier,
        ..Fields.SelectMany(field => field.GetTokens())
    ];
}