using Spectre.Console;

var result = evaluateFile("./Heir/Test.heir") ?? "none";
Console.WriteLine(result); // TODO: some sort of repr function

object? evaluateFile(string filePath)
{
    var lexer = Heir.Lexer.FromFile(filePath);
    var tokenStream = lexer.GetTokens();
    var parser = new Heir.Parser(tokenStream);
    var syntaxTree = parser.Parse();
    var binder = new Heir.Binder(syntaxTree);
    var boundSyntaxTree = binder.Bind();
    //Console.WriteLine(tokenStream.ToString());
    //syntaxTree.Display();
    //boundSyntaxTree.Display();
    //Console.WriteLine();
    if (boundSyntaxTree.Diagnostics.HasErrors())
    {
        AnsiConsole.MarkupLine(boundSyntaxTree.Diagnostics.ToString(true));
        return null;
    }

    var bytecodeGenerator = new Heir.BytecodeGenerator(binder, syntaxTree);
    var bytecode = bytecodeGenerator.GenerateBytecode();
    var vm = new Heir.VirtualMachine(binder, bytecode);
    var result = vm.Evaluate();

    AnsiConsole.MarkupLine(vm.Diagnostics.ToString(true));
    Console.WriteLine();
    //Console.WriteLine(bytecode.ToString());
    //Console.WriteLine();
    return result;
}
