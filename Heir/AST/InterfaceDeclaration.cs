using Heir.Syntax;
using Heir.AST.Abstract;
using Heir.Binding;
using Heir.Types;

namespace Heir.AST;

public class InterfaceDeclaration(Token keyword, Token identifier, List<InterfaceField> fields) : Statement
{
    public Token Keyword { get; } = keyword;
    public Token Identifier { get; } = identifier;
    public List<InterfaceField> Fields { get; } = fields;
    public InterfaceType Type { get; } = new(
    fields
        .ConvertAll(field => new KeyValuePair<LiteralType, InterfaceMemberSignature>(
            new(field.Identifier.Text),
            new(BaseType.FromTypeRef(field.Type), field.IsMutable)))
        .ToDictionary(),
        [],
        identifier.Text
    );
    
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitInterfaceDeclaration(this);

    public override List<Token> GetTokens() =>
    [
        Keyword,
        Identifier,
        ..Fields.SelectMany(field => field.GetTokens())
    ];
}