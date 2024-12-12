using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class SyntaxTree(SyntaxNode[] statements) : SyntaxNode
    {
        public SyntaxNode[] Statements { get; } = statements;

        public override void Display(int indent = 0)
        {
            foreach (var statement in Statements)
                statement.Display(indent);
        }

        public override List<Instruction> GenerateBytecode() =>
            Statements
            .Select(statement => statement.GenerateBytecode())
            .Aggregate((finalBytecode, statementBytecode) => finalBytecode.Concat(statementBytecode).ToList());

        public override List<Token> GetTokens()
        {
            throw new NotImplementedException();
        }
    }
}
