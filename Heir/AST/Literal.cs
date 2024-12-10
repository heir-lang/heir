using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class Literal(Token token) : Expression
    {
        public Token Token { get; } = token;

        public override List<Instruction> GenerateBytecode() => [new Instruction(this, OpCode.PUSH, Token.Value)];

        public override List<Token> GetTokens() => [Token];

        public override void Display(int indent)
        {
            var valueText = Token.Value?.ToString() ?? "none";
            if (Token.IsKind(SyntaxKind.BoolLiteral))
                valueText = valueText.ToLower();

            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}Literal({Token.Kind}, {valueText})");
        }
    }
}
