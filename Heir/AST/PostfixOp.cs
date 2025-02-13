using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class PostfixOp(Expression operand, Token op) : Expression
{
    public Expression Operand { get; } = operand;
    public Token Operator { get; } = op;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitPostfixOpExpression(this);
    public override List<Token> GetTokens() => Operand.GetTokens().Append(Operator).ToList();
}