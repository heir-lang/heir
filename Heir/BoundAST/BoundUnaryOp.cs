using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundUnaryOp(BoundUnaryOperator op, BoundExpression operand) : BoundExpression
{
    public override BaseType Type => Operator.ResultType;
    public BoundUnaryOperator Operator { get; } = op;
    public BoundExpression Operand { get; } = operand;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundUnaryOpExpression(this);
    public override List<Token> GetTokens() => Operand.GetTokens();
}