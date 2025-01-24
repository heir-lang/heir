using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class UnaryOp(Expression operand, Token op) : Expression
{
    public Expression Operand { get; } = operand;
    public Token Operator { get; } = op;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitUnaryOpExpression(this);
    public override List<Token> GetTokens() => Operand.GetTokens().Append(Operator).ToList();
}