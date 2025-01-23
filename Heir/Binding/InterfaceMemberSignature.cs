using Heir.Types;

namespace Heir.Binding;

public class InterfaceMemberSignature(BaseType valueType, bool isMutable = false)
{
    public BaseType ValueType { get; } = valueType;
    public bool IsMutable { get; } = isMutable;
}