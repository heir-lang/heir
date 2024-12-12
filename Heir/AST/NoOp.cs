using Heir.Syntax;

namespace Heir.AST
{
    public class NoOp : Expression
    {
        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitNoOp(this);
        public override List<Token> GetTokens() => [];
        public override void Display(int indent) => Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundNoOp");
    }
}
