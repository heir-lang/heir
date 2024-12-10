using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class IdentifierName(Token token) : Name
    {
        public Token Token { get; } = token;

        public override List<Instruction> GenerateBytecode() => [new Instruction(this, OpCode.LOAD, Token.Text)];
        public override List<Token> GetTokens() => [Token];

        public override void Display(int indent)
        {
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}IdentifierName({Token.Text})");
        }
    }
}
