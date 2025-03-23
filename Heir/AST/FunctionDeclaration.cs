using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class FunctionDeclaration(
    Token keyword,
    IdentifierName name,
    List<Parameter> parameters,
    List<TypeParameter> typeParameters,
    Block body,
    TypeRef? returnType) : Statement
{
    public Token Keyword { get; } = keyword;
    public IdentifierName Name { get; } = name;
    public List<Parameter> Parameters { get; } = parameters;
    public List<TypeParameter> TypeParameters { get; } = typeParameters;
    public Block Body { get; } = body;
    public TypeRef? ReturnType { get; } = returnType;
    
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitFunctionDeclaration(this);
    public override List<Token> GetTokens() =>
    [
        Keyword,
        Name.Token,
        ..Parameters.SelectMany(parameter => parameter.GetTokens()).ToList(),
        ..Body.GetTokens(),
        ..ReturnType?.GetTokens() ?? []
    ];
}