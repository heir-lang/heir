using Heir.AST;
using Heir.CodeGeneration;

namespace Heir.Runtime.Values;

public sealed class Function
{
    public unsafe int* MemoryAddress { get; }
    public string Name => _declaration.Name.Token.Text;

    private readonly FunctionDeclaration _declaration;
    private readonly List<Instruction> _bodyBytecode;
    private readonly Scope _closure;
    
    public unsafe Function(FunctionDeclaration declaration, List<Instruction> bodyBytecode, Scope closure)
    {
        _declaration = declaration;
        _bodyBytecode = bodyBytecode;
        _closure = closure;
        
        fixed (int* ptr = &Call)
        {
            MemoryAddress = ptr;
        }
    }

    public object? Call(VirtualMachine vm, List<object?> arguments)
    {
        var scope = new Scope(_closure);
        var index = 0;
        foreach (var parameter in _declaration.Parameters)
        {
            var defaultValue = parameter.Initializer?.Token.Value;
            var value = arguments.ElementAtOrDefault(index++) ?? defaultValue;
            scope.Define(parameter.Name.Token.Text, value);
        }
        
        vm.BeginRecursion(_declaration.Name.Token);
        var closureVM = new VirtualMachine(new(_bodyBytecode, vm.Diagnostics), scope, vm.RecursionDepth);
        var result = closureVM.Evaluate();
        vm.EndRecursion();

        return result;
    }
}