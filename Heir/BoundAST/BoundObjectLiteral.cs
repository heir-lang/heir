﻿using Heir.BoundAST.Abstract;
using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST;

public sealed class BoundObjectLiteral(Token brace, Dictionary<BaseType, BoundExpression> properties, InterfaceType type) : BoundExpression
{
    public Token Token { get; } = brace;
    public override InterfaceType Type => type;
    public Dictionary<BaseType, BoundExpression> Properties { get; } = properties;

    public override List<Token> GetTokens() => [Token, ..Properties.Values.SelectMany(expr => expr.GetTokens())];
    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBoundObjectLiteralExpression(this);
}