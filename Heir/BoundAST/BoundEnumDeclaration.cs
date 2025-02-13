using Heir.Binding;
using Heir.Syntax;
using Heir.BoundAST.Abstract;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundEnumDeclaration(
    Token keyword,
    VariableSymbol<InterfaceType> symbol,
    HashSet<BoundEnumMember> members,
    bool isInline) : BoundStatement
{
    public override BaseType Type => Symbol.Type;
    
    public TypeSymbol TypeSymbol => new(Symbol.Name, new UnionType(MemberTypes));
    public List<BaseType> MemberTypes { get; } = members.Select(member => member.Type).OfType<BaseType>().ToList();
    
    public Token Keyword { get; } = keyword;
    public VariableSymbol<InterfaceType> Symbol { get; } = symbol;
    public HashSet<BoundEnumMember> Members { get; } = members;
    public bool IsInline { get; } = isInline;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundEnumDeclaration(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        Symbol.Name,
        ..Members.SelectMany(member => member.GetTokens())
    ];
}