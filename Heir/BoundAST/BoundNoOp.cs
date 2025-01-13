using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public class BoundNoOp : BoundExpression
{
    public override PrimitiveType Type => PrimitiveType.None;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundNoOp(this);
    public override List<Token> GetTokens() => [];
    public override void Display(int indent = 0) =>
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundNoOp");
}