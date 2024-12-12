using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class UnaryOp(SyntaxNode operand, Token op) : Expression
    {
        public SyntaxNode Operand { get; } = operand;
        public Token Operator { get; } = op;

        public List<Instruction> GenerateBytecode()
        {
            var value = Operand.GenerateBytecode();
            var bytecode = Operator.Kind switch
            {
                SyntaxKind.Bang => value.Append(new Instruction(this, OpCode.NOT)),
                SyntaxKind.Tilde => value.Append(new Instruction(this, OpCode.BNOT)),
                SyntaxKind.Minus => value.Append(new Instruction(this, OpCode.UNM)),
                SyntaxKind.PlusPlus => value.Append(new Instruction(this, OpCode.PUSH, 1)).Append(new Instruction(this, OpCode.ADD)),
                SyntaxKind.MinusMinus => value.Append(new Instruction(this, OpCode.PUSH, 1)).Append(new Instruction(this, OpCode.SUB)),

                _ => null!
            };

            return bytecode.ToList();
        }

        public override List<Token> GetTokens() => Operand.GetTokens().Append(Operator).ToList();

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}Unary(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operand ->");
            Operand.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
