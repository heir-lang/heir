using Heir.AST;
using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir;

public sealed class CompileTimeMacroEvaluator(SyntaxTree cleanTree) : NodeTransformer(cleanTree)
{
    // TODO: (important) scoping
    private readonly Dictionary<string, Expression> _inlinedValues = [];
    private readonly List<EnumDeclaration> _inlinedEnums = [];
    
    public SyntaxTree Evaluate() => Transform();
    
    public override SyntaxNode? VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        if (enumDeclaration.IsInline)
        {
            _inlinedEnums.Add(enumDeclaration);
            return new NoOpStatement();
        }
        
        return base.VisitEnumDeclaration(enumDeclaration);
    }

    public override SyntaxNode? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        if (variableDeclaration.IsInline)
        {
            _inlinedValues.TryAdd(variableDeclaration.Name.Token.Text, variableDeclaration.Initializer!);
            return new NoOpStatement();
        }
        
        return base.VisitVariableDeclaration(variableDeclaration);
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccess memberAccess)
    {
        if (memberAccess.Expression is not IdentifierName name)
            return base.VisitMemberAccessExpression(memberAccess);

        var enumDeclaration = _inlinedEnums.FirstOrDefault(declaration => declaration.Name.ToString() == name.ToString());
        var member = enumDeclaration?.Members.FirstOrDefault(member => member.Name.ToString() == memberAccess.Name.ToString());
        if (member == null || enumDeclaration == null)
            return base.VisitMemberAccessExpression(memberAccess);

        return member.Value;
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