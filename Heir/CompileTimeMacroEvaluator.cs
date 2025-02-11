using Heir.AST;
using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir;

public sealed class CompileTimeMacroEvaluator(SyntaxTree cleanTree) : NodeTransformer(cleanTree)
{
    private readonly Dictionary<string, Expression> _inlinedValues = [];
    
    public SyntaxTree Evaluate() => Transform();

    public override SyntaxNode? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        if (variableDeclaration.IsInline)
        {
            _inlinedValues.TryAdd(variableDeclaration.Name.Token.Text, variableDeclaration.Initializer!);
            return new NoOpStatement();
        }
        
        return base.VisitVariableDeclaration(variableDeclaration);
    }

    public override SyntaxNode? VisitIdentifierNameExpression(IdentifierName identifierName)
    {
        return _inlinedValues.TryGetValue(identifierName.Token.Text, out var valueExpression)
            ? valueExpression
            : base.VisitIdentifierNameExpression(identifierName);
    }

    public override Literal VisitNameOfExpression(NameOf nameOf) => 
        new(TokenFactory.StringFromIdentifier(nameOf.Name.Token));
}