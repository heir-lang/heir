using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundNoOpStatement : BoundStatement
{
    public override BaseType? Type => null;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundNoOp(this);
    public override List<Token> GetTokens() => [];
}