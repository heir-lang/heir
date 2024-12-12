using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundNoOp : BoundExpression
    {
        public override PrimitiveType Type => new PrimitiveType(PrimitiveTypeKind.None);

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundNoOp(this);
        public override List<Token> GetTokens() => [];
        public override void Display(int indent) => Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundNoOp");
    }
}
