using Heir.Syntax;

namespace Heir.Runtime.Intrinsics;

public class IntrinsicRegistrar(IIntrinsicValue intrinsicValue)
{
    public bool IsGlobal { get; } = intrinsicValue.IsGlobal;
    
    private readonly Token _nameToken = TokenFactory.Identifier(intrinsicValue.Name, Location.Intrinsic, Location.Intrinsic);
    
    public void RegisterInResolver(Resolver resolver)
    {
        resolver.Declare(_nameToken);
        resolver.Define(_nameToken);
    }

    public void RegisterSymbol(Binder binder) =>
        binder.DefineSymbol(_nameToken, intrinsicValue.Type, false);
    
    public void RegisterValue(Scope scope) =>
        scope.Define(intrinsicValue.Name, intrinsicValue.Value);
}