using Heir.Syntax;
using Heir.AST;
using Heir.CodeGeneration;
using Heir.Types;
using Heir.BoundAST;

namespace Heir
{
    public sealed class BytecodeGenerator(Binder binder, SyntaxTree syntaxTree) : Statement.Visitor<List<Instruction>>, Expression.Visitor<List<Instruction>>
    {
        public DiagnosticBag Diagnostics { get; } = binder.Diagnostics;

        private readonly Binder _binder = binder;
        private readonly SyntaxTree _syntaxTree = syntaxTree;

        public List<Instruction> GenerateBytecode() => GenerateBytecode(_syntaxTree);

        public List<Instruction> VisitSyntaxTree(SyntaxTree syntaxTree) => GenerateStatementsBytecode(syntaxTree.Statements);
        // TODO: create scope
        public List<Instruction> VisitBlock(Block block) => GenerateStatementsBytecode(block.Statements);

        public List<Instruction> VisitAssignmentOpExpression(AssignmentOp assignmentOp) => VisitBinaryOpExpression(assignmentOp);
        public List<Instruction> VisitBinaryOpExpression(BoundBinaryOp binaryOp)
        {
            var leftBoundNode = _binder.GetBoundNode(binaryOp.Left);
            var rightBoundNode = _binder.GetBoundNode(binaryOp.Right);

            List<Instruction> cannotApply()
            {
                Diagnostics.Error("H010", $"Cannot apply operator \"{binaryOp.Operator.Text}\" to operands of type \"{leftBoundNode.Type.ToString()}\" and \"{rightBoundNode.Type.ToString()}\"", binaryOp.Operator);
                return [];
            }
            bool isNumber(BoundExpression boundExpression)
            {
                return boundExpression.Type.IsAssignableTo(new PrimitiveType(PrimitiveTypeKind.Int))
                    || boundExpression.Type.IsAssignableTo(new PrimitiveType(PrimitiveTypeKind.Float));
            }

            if (SyntaxFacts.NumberOperators.Contains(binaryOp.Operator.Kind) && (!isNumber(leftBoundNode) && !isNumber(rightBoundNode)))
                return cannotApply();
            else if (!leftBoundNode.Type.IsAssignableTo(rightBoundNode.Type))
                return cannotApply();

            var leftInstructions = GenerateBytecode(binaryOp.Left);
            var rightInstructions = GenerateBytecode(binaryOp.Right);
            var combined = leftInstructions.Concat(rightInstructions);

            if (BoundBinaryOp.StandardOpCodeMap.TryGetValue(binaryOp.Operator.Kind, out var standardOp))
                return combined.Append(new Instruction(binaryOp, standardOp)).ToList();

            if (BoundBinaryOp.AssignmentOpCodeMap.TryGetValue(binaryOp.Operator.Kind, out var assignmentOp))
                return leftInstructions
                    .Concat(rightInstructions)
                    .Append(new Instruction(binaryOp, assignmentOp))
                    .Append(new Instruction(binaryOp, OpCode.STORE))
                    .ToList();

            // temp
            throw new NotSupportedException($"Unsupported operator kind: {binaryOp.Operator.Kind}");
        }

        public List<Instruction> VisitUnaryOpExpression(UnaryOp unaryOp)
        {
            var value = GenerateBytecode(unaryOp.Operand);
            var bytecode = unaryOp.Operator.Kind switch
            {
                SyntaxKind.Bang => value.Append(new Instruction(unaryOp, OpCode.NOT)),
                SyntaxKind.Tilde => value.Append(new Instruction(unaryOp, OpCode.BNOT)),
                SyntaxKind.Minus => value.Append(new Instruction(unaryOp, OpCode.UNM)),
                SyntaxKind.PlusPlus => value.Append(new Instruction(unaryOp, OpCode.PUSH, 1)).Append(new Instruction(unaryOp, OpCode.ADD)),
                SyntaxKind.MinusMinus => value.Append(new Instruction(unaryOp, OpCode.PUSH, 1)).Append(new Instruction(unaryOp, OpCode.SUB)),

                _ => null!
            };

            return bytecode.ToList();
        }

        public List<Instruction> VisitIdentifierNameExpression(IdentifierName identifierName) => [new Instruction(identifierName, OpCode.LOAD, identifierName.Token.Text)];
        public List<Instruction> VisitParenthesizedExpression(Parenthesized parenthesized) => GenerateBytecode(parenthesized.Expression);
        public List<Instruction> VisitLiteralExpression(Literal literal) => [new Instruction(literal, OpCode.PUSH, literal.Token.Value)];
        public List<Instruction> VisitNoOp(NoOp noOp) => [new Instruction(noOp, OpCode.NOOP)];

        //private List<Instruction> GenerateStatementsBytecode(List<Statement> statements) => statements.SelectMany(GenerateBytecode).ToList();
        private List<Instruction> GenerateStatementsBytecode(List<SyntaxNode> statements) => statements.SelectMany(GenerateBytecode).ToList(); // temp
        private List<Instruction> GenerateBytecode(Expression expression) => expression.Accept(this);
        private List<Instruction> GenerateBytecode(Statement statement) => statement.Accept(this);
        private List<Instruction> GenerateBytecode(SyntaxNode node)
        {
            if (node is Expression expression)
                return GenerateBytecode(expression);
            else if (node is Statement statement)
                return GenerateBytecode(statement);

            return null!; // poop
        }
    }
}
