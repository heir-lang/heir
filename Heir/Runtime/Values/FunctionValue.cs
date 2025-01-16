using Heir.AST;
using Heir.CodeGeneration;

namespace Heir.Runtime.Values;

public sealed class FunctionValue(FunctionDeclaration declaration, List<Instruction> bodyBytecode, Scope closure)
{
    public Guid ID { get; } = Guid.NewGuid();
    public string Name { get; } = declaration.Name.Token.Text;
    public FunctionDeclaration Declaration { get; } = declaration;
    public List<Instruction> BodyBytecode { get; } = bodyBytecode;
    public Scope Closure { get; } = closure;

    public override string ToString() => $"<function: {ID}>";
}