using Heir.Types;

namespace Heir.Binding;

public class InterfaceMemberSignature(BaseType type, bool isMutable = false)
{
    public BaseType Type { get; } = type;
    public bool IsMutable { get; } = isMutable;
    
    public override string ToString() => (IsMutable ? "mut ": "") + Type.ToString();
}