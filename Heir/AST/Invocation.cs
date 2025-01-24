using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class Invocation(Expression callee, List<Expression> arguments) : Expression
{
    public Expression Callee { get; } = callee;
    public List<Expression> Arguments { get; } = arguments;

    public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitInvocationExpression(this);
    public override List<Token> GetTokens() => 
    [
        ..Callee.GetTokens(),
        ..Arguments.SelectMany(argument => argument.GetTokens())
    ];
}