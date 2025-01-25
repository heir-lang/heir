using Heir.AST;
using Heir.Syntax;

namespace Heir;

public sealed class CompileTimeMacroEvaluator(SyntaxTree cleanTree) : NodeTransformer(cleanTree)
{
    public SyntaxTree Evaluate() => Transform();
    
    public override Literal VisitNameOfExpression(NameOf nameOf) => 
        new(TokenFactory.StringFromIdentifier(nameOf.Name.Token));
}