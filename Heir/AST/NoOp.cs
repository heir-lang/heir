using Heir.Syntax;

namespace Heir.AST
{
    public class NoOp : Expression
    {
        public override List<Token> GetTokens() => [];

        public override void Display(int indent)
        {
        }
    }
}
