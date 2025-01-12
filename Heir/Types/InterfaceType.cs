using Heir.Binding;
using System.Text;

namespace Heir.Types;

public sealed class InterfaceType(
    Dictionary<LiteralType, InterfaceMemberSignature> members,
    Dictionary<PrimitiveType, BaseType> indexSignatures,
    string? name = null
) : SingularType(name ?? _defaultInterfaceName)
{
    private const string _defaultInterfaceName = "Object";

    public Dictionary<LiteralType, InterfaceMemberSignature> Members { get; } = members;
    public Dictionary<PrimitiveType, BaseType> IndexSignatures { get; } = indexSignatures;
        
    public string ToString(bool colors, int indent = 0)
    {
        var result = new StringBuilder(Name == _defaultInterfaceName ? "" : Name + " ");
        result.Append('{');
        if (IndexSignatures.Count > 0 || Members.Count > 0)
            indent++;

        foreach (var signature in IndexSignatures)
        {
            result.AppendLine();
            result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
            result.Append('[');
            result.Append(signature.Key.ToString());
            result.Append("]: ");
            result.Append(signature.Value is InterfaceType interfaceType ? interfaceType.ToString(colors, indent + 1) : signature.Value.ToString(colors));
            result.Append(';');
        }

        foreach (var member in Members)
        {
            result.AppendLine();
            result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
            result.Append(member.Value.IsMutable ? "mut " : "");
            result.Append(member.Value.ValueType is InterfaceType interfaceType ? interfaceType.ToString(colors, indent + 1) : member.Value.ValueType.ToString(colors));
            result.Append(member.Key.ToString(colors));
            result.Append(';');
        }

        result.AppendLine();
        result.Append('}');
        return result.ToString();
    }
}