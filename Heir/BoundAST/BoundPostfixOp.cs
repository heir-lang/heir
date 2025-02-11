using Heir.Binding;
using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundPostfixOp(BoundPostfixOperator op, BoundExpression operand) : BoundExpression
{
    public override BaseType Type => Operator.ResultType;
    public BoundPostfixOperator Operator { get; } = op;
    public BoundExpression Operand { get; } = operand;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundPostfixOpExpression(this);
    public override List<Token> GetTokens() => Operand.GetTokens();
}