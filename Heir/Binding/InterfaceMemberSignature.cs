using Heir.Types;

namespace Heir.Binding
{
    public class InterfaceMemberSignature(BaseType valueType, bool isMutable = false) : InterfaceMemberSignature<BaseType>(valueType, isMutable);

    public class InterfaceMemberSignature<T>(T valueType, bool isMutable) where T : BaseType
    {
        public T ValueType { get; } = valueType;
        public bool IsMutable { get; } = isMutable;
    }
}
