﻿using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class NoOpType : TypeRef
{
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitNoOp(this);
    public override List<Token> GetTokens() => [];
}