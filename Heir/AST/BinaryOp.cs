using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class BinaryOp(SyntaxNode left, Token op, SyntaxNode right) : Expression
    {
        public SyntaxNode Left { get; } = left;
        public Token Operator { get; } = op;
        public SyntaxNode Right { get; } = right;

        private static readonly Dictionary<SyntaxKind, OpCode> StandardOpMap = new()
        {
            { SyntaxKind.Plus,                  OpCode.ADD },
            { SyntaxKind.Minus,                 OpCode.SUB },
            { SyntaxKind.Star,                  OpCode.MUL },
            { SyntaxKind.Slash,                 OpCode.DIV },
            { SyntaxKind.SlashSlash,            OpCode.IDIV },
            { SyntaxKind.Percent,               OpCode.MOD },
            { SyntaxKind.Carat,                 OpCode.POW },
            { SyntaxKind.Ampersand,             OpCode.BAND },
            { SyntaxKind.Pipe,                  OpCode.BOR },
            { SyntaxKind.Tilde,                 OpCode.BXOR },
            { SyntaxKind.AmpersandAmpersand,    OpCode.AND },
            { SyntaxKind.PipePipe,              OpCode.OR }
        };

        private static readonly Dictionary<SyntaxKind, OpCode> AssignmentOpMap = new()
        {
            { SyntaxKind.PlusEquals,                OpCode.ADD },
            { SyntaxKind.MinusEquals,               OpCode.SUB },
            { SyntaxKind.StarEquals,                OpCode.MUL },
            { SyntaxKind.SlashEquals,               OpCode.DIV },
            { SyntaxKind.SlashSlashEquals,          OpCode.IDIV },
            { SyntaxKind.PercentEquals,             OpCode.MOD },
            { SyntaxKind.CaratEquals,               OpCode.POW },
            { SyntaxKind.AmpersandEquals,           OpCode.BAND },
            { SyntaxKind.PipeEquals,                OpCode.BOR },
            { SyntaxKind.TildeEquals,               OpCode.BXOR },
            { SyntaxKind.AmpersandAmpersandEquals,  OpCode.AND },
            { SyntaxKind.PipePipeEquals,            OpCode.OR }
        };

        public List<Instruction> GenerateBytecode()
        {
            var leftInstructions = Left.GenerateBytecode();
            var rightInstructions = Right.GenerateBytecode();
            var combined = leftInstructions.Concat(rightInstructions);

            if (StandardOpMap.TryGetValue(Operator.Kind, out var standardOp))
                return combined.Append(new Instruction(this, standardOp)).ToList();

            if (AssignmentOpMap.TryGetValue(Operator.Kind, out var assignmentOp))
                return leftInstructions
                    .Concat(rightInstructions)
                    .Append(new Instruction(this, assignmentOp))
                    .Append(new Instruction(this, OpCode.STORE))
                    .ToList();

            throw new NotSupportedException($"Unsupported operator kind: {Operator.Kind}");
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
