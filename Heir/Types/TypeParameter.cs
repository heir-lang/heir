namespace Heir.Types;

public class TypeParameter(string name, BaseType? baseType, BaseType? initializer) : SingularType(name)
{
    public BaseType? BaseType { get; } = baseType;
    public BaseType? Initializer { get; } = initializer;
    
    public override string ToString() =>
        $"{Name}{(BaseType != null ? " : " + BaseType.ToString() : "")}{(Initializer != null && Initializer is not AnyType ? " = " + Initializer.ToString() : "")}";
}