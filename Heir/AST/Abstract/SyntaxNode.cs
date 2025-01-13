using Heir.Syntax;

namespace Heir.AST.Abstract;

public abstract class SyntaxNode
{
    public abstract void Display(int indent = 0);
    public abstract List<Token> GetTokens();

    public Token GetFirstToken() => GetTokens().First();
    public Token GetLastToken() => GetTokens().Last();
}