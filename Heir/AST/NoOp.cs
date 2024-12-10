using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class NoOp : Expression
    {
        public override List<Instruction> GenerateBytecode() => [new Instruction(this, OpCode.NOOP)];
        public override List<Token> GetTokens() => [];

        public override void Display(int indent)
        {
        }
    }
}
