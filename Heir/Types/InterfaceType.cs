using Heir.Binding;
using System.Text;

namespace Heir.Types;

// TODO: type params
public class InterfaceType(
    Dictionary<LiteralType, InterfaceMemberSignature> members,
    Dictionary<PrimitiveType, BaseType> indexSignatures,
    string? name = null
) : SingularType(name ?? _defaultInterfaceName)
{
    private const string _defaultInterfaceName = "Object";

    public Dictionary<LiteralType, InterfaceMemberSignature> Members { get; } = members;
    public Dictionary<PrimitiveType, BaseType> IndexSignatures { get; } = indexSignatures;
    public BaseType IndexType { get; } = members.Count + indexSignatures.Count == 1
        ? members.Keys.FirstOrDefault() ?? indexSignatures.Keys.First()
        : new UnionType(new List<BaseType>([..members.Keys, ..indexSignatures.Keys]).Distinct().ToList());

    /// <summary>Creates an interface with immutable fields & no custom index signatures; just members </summary>
    public static InterfaceType Readonly(string name, Dictionary<string, BaseType> members) =>
        new(
            members.Select(pair =>
                new KeyValuePair<LiteralType, InterfaceMemberSignature>(new(pair.Key), new(pair.Value))
            ).ToDictionary(),
            [],
            name
        );
        
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
            result.Append('[' + (colors ? "[" : ""));
            result.Append(signature.Key.ToString());
            result.Append((colors ? "]" : "") + "]: ");
            result.Append(signature.Value is InterfaceType interfaceType ? interfaceType.ToString(colors, indent + 1) : signature.Value.ToString(colors));
            result.Append(';');
        }

        foreach (var member in Members)
        {
            result.AppendLine();
            result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
            result.Append(member.Value.IsMutable ? "mut " : "");
            result.Append(member.Key.Value);
            result.Append(": ");
            result.Append(member.Value.Type is InterfaceType interfaceType ? interfaceType.ToString(colors, indent + 1) : member.Value.Type.ToString(colors));
            result.Append(';');
        }

        indent--;
        result.AppendLine();
        result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
        result.Append('}');
        return result.ToString();
    }
}