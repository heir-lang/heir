using Heir.AST;
using Heir.CodeGeneration;

namespace Heir.Runtime.Values;

public sealed class Function(FunctionDeclaration declaration, List<Instruction> bodyBytecode)
{
    public Guid ID { get; } = Guid.NewGuid();
    public string Name => declaration.Name.Token.Text;

    public object? Call(VirtualMachine vm, List<List<Instruction>> argumentsBytecode)
    {
        var closure = new Scope(vm.Scope);
        for (var i = 0; i < declaration.Parameters.Count; i++)
        {
            var parameter = declaration.Parameters[i];
            var defaultValue = parameter.Initializer?.Token.Value;
            var bytecode = i < argumentsBytecode.Count ? argumentsBytecode[i] : null;
            var argumentsVM = i < argumentsBytecode.Count
                ? new VirtualMachine(new(bytecode!, vm.Diagnostics), closure, vm.RecursionDepth)
                : null;
            
            var value = argumentsVM?.Evaluate() ?? defaultValue;
            closure.Define(parameter.Name.Token.Text, value);
        }
        
        vm.BeginRecursion(declaration.Name.Token);
        var closureVM = new VirtualMachine(new(bodyBytecode, vm.Diagnostics), closure, vm.RecursionDepth);
        var result = closureVM.Evaluate();
        vm.EndRecursion();

        return result;
    }
}