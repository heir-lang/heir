﻿using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class UnionType(List<TypeRef> types) : TypeRef
{
    public List<TypeRef> Types { get; } = types;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitUnionTypeRef(this);
    public override List<Token> GetTokens() => Types.SelectMany(type => type.GetTokens()).ToList();
}