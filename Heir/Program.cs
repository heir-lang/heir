using Spectre.Console;

var result = evaluateFile("./Heir/Test.heir") ?? "none";
Console.WriteLine(result);

object? evaluateFile(string filePath)
{
    var lexer = Heir.Lexer.FromFile(filePath);
    var tokenStream = lexer.GetTokens();
    var parser = new Heir.Parser(tokenStream);
    var syntaxTree = parser.Parse();
    var binder = new Heir.Binder(syntaxTree);
    var boundSyntaxTree = binder.Bind();
    var bytecodeGenerator = new Heir.BytecodeGenerator(binder, syntaxTree);
    var bytecode = bytecodeGenerator.GenerateBytecode();
    var vm = new Heir.VirtualMachine(binder, bytecode);
    var result = vm.Evaluate();

    Console.WriteLine("Diagnostics:");
    AnsiConsole.MarkupLine(vm.Diagnostics.ToString(true));

    Console.WriteLine();
    syntaxTree.Display();
    //boundSyntaxTree.Display();
    Console.WriteLine();
    Console.WriteLine();
    return result;
}
