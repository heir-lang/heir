using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundNoOp(BaseType? type = null) : BoundExpression
{
    public override BaseType Type => type ?? PrimitiveType.None;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundNoOp(this);
    public override List<Token> GetTokens() => [];
}