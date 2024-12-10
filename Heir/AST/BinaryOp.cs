using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class BinaryOp(SyntaxNode left, Token op, SyntaxNode right) : Expression
    {
        public SyntaxNode Left { get; } = left;
        public Token Operator { get; } = op;
        public SyntaxNode Right { get; } = right;

        public override List<Instruction> GenerateBytecode()
        {
            var left = Left.GenerateBytecode();
            var right = Right.GenerateBytecode();
            var combined = left.Concat(right);
            var bytecode = GetStandardOperations(combined) ?? (Operator.Kind switch
            {
                SyntaxKind.PlusEquals => left.Concat(combined.Append(new Instruction(this, OpCode.ADD))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.MinusEquals => left.Concat(combined.Append(new Instruction(this, OpCode.SUB))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.StarEquals => left.Concat(combined.Append(new Instruction(this, OpCode.MUL))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.SlashEquals => left.Concat(combined.Append(new Instruction(this, OpCode.DIV))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.SlashSlashEquals => left.Concat(combined.Append(new Instruction(this, OpCode.IDIV))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.PercentEquals => left.Concat(combined.Append(new Instruction(this, OpCode.MOD))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.CaratEquals => left.Concat(combined.Append(new Instruction(this, OpCode.POW))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.AmpersandEquals => left.Concat(combined.Append(new Instruction(this, OpCode.BAND))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.PipeEquals => left.Concat(combined.Append(new Instruction(this, OpCode.BOR))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.TildeEquals => left.Concat(combined.Append(new Instruction(this, OpCode.BXOR))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.AmpersandAmpersandEquals => left.Concat(combined.Append(new Instruction(this, OpCode.AND))).Append(new Instruction(this, OpCode.STORE)),
                SyntaxKind.PipePipeEquals => left.Concat(combined.Append(new Instruction(this, OpCode.OR))).Append(new Instruction(this, OpCode.STORE)),

                _ => null!
            });

            return bytecode.ToList();
        }

        private IEnumerable<Instruction>? GetStandardOperations(IEnumerable<Instruction> combined)
        {
            return Operator.Kind switch
            {
                SyntaxKind.Plus => combined.Append(new Instruction(this, OpCode.ADD)),
                SyntaxKind.Minus => combined.Append(new Instruction(this, OpCode.SUB)),
                SyntaxKind.Star => combined.Append(new Instruction(this, OpCode.MUL)),
                SyntaxKind.Slash => combined.Append(new Instruction(this, OpCode.DIV)),
                SyntaxKind.SlashSlash => combined.Append(new Instruction(this, OpCode.IDIV)),
                SyntaxKind.Percent => combined.Append(new Instruction(this, OpCode.MOD)),
                SyntaxKind.Carat => combined.Append(new Instruction(this, OpCode.POW)),
                SyntaxKind.Ampersand => combined.Append(new Instruction(this, OpCode.BAND)),
                SyntaxKind.Pipe => combined.Append(new Instruction(this, OpCode.BOR)),
                SyntaxKind.Tilde => combined.Append(new Instruction(this, OpCode.BXOR)),
                SyntaxKind.AmpersandAmpersand => combined.Append(new Instruction(this, OpCode.AND)),
                SyntaxKind.PipePipe => combined.Append(new Instruction(this, OpCode.OR)),

                _ => null
            };
        }

        public override List<Token> GetTokens() => Left.GetTokens().Append(Operator).Concat(Right.GetTokens()).ToList();

        public override void Display(int indent)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BinaryOp(");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Left ->");
            Left.Display(indent + 2);
            Console.WriteLine(",");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Right ->");
            Right.Display(indent + 2);
            Console.WriteLine();
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
