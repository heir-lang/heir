using Heir.AST;

namespace Heir;

public sealed class CompileTimeMacroEvaluator(SyntaxTree cleanTree) : NodeTransformer(cleanTree)
{
    public SyntaxTree Evaluate() => Transform();
}