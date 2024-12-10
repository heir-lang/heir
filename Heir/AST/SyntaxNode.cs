using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public abstract class Statement : SyntaxNode
    {
    }

    public abstract class Expression : SyntaxNode
    {
    }

    public abstract class SyntaxNode
    {
        public abstract List<Instruction> GenerateBytecode();
        public abstract void Display(int indent = 0);
        public abstract List<Token> GetTokens();

        public Token GetFirstToken() => GetTokens().First();
        public Token GetLastToken() => GetTokens().Last();
        public bool Is<T>() where T : SyntaxNode => this is T;
    }
}
