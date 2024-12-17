using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public class BoundExpressionStatement(BoundExpression expression) : BoundStatement
    {
        public override BaseType? Type => Expression.Type;

        public BoundExpression Expression { get; } = expression;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundExpressionStatement(this);
        public override List<Token> GetTokens() => [];
        public override void Display(int indent) => Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundNoOpStatement");
    }
}
