using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.AST;

public class InterfaceField(Token identifier, TypeRef type, bool isMutable) : Statement
{
    public Token Identifier { get; } = identifier;
    public TypeRef Type { get; } = type;
    public bool IsMutable { get; } = isMutable;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitInterfaceField(this);
    public override List<Token> GetTokens() =>
    [
        Identifier,
        ..Type.GetTokens()
    ];
}