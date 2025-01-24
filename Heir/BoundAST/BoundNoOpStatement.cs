using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundNoOpStatement(BaseType? type = null) : BoundStatement
{
    public override BaseType? Type => type;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundNoOp(this);
    public override List<Token> GetTokens() => [];
}