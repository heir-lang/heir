using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class BinaryOp(Expression left, Token op, Expression right) : Expression
{
    public Expression Left { get; } = left;
    public Token Operator { get; } = op;
    public Expression Right { get; } = right;
        
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBinaryOpExpression(this);
    public override List<Token> GetTokens() => Left.GetTokens().Append(Operator).Concat(Right.GetTokens()).ToList();
}