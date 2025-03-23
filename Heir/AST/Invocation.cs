using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class Invocation(Expression callee, List<Expression> arguments, List<TypeRef> typeArguments) : Expression
{
    public Expression Callee { get; } = callee;
    public List<Expression> Arguments { get; } = arguments;
    public List<TypeRef> TypeArguments { get; } = typeArguments;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitInvocationExpression(this);
    public override List<Token> GetTokens() => 
    [
        ..Callee.GetTokens(),
        ..Arguments.SelectMany(argument => argument.GetTokens())
    ];
}