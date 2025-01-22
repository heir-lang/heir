using Heir.Syntax;

namespace Heir.AST.Abstract;

public abstract class SyntaxNode
{
    public abstract List<Token> GetTokens();

    public Token GetFirstToken() => GetTokens().First();
    public Token GetLastToken() => GetTokens().Last();
}