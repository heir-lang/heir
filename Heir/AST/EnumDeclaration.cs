using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.AST;

public sealed class EnumDeclaration(Token keyword, IdentifierName name, HashSet<EnumMember> members, bool isInline) : Statement
{
    public Token Keyword { get; } = keyword;
    public IdentifierName Name { get; } = name;
    public HashSet<EnumMember> Members { get; } = members;
    public bool IsInline { get; } = isInline;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitEnumDeclaration(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        ..Name.GetTokens(),
        ..Members.SelectMany(member => member.GetTokens())
    ];
}