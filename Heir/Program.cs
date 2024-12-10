Console.WriteLine(evaluateFile("./Heir/Test.heir") ?? "null");

object? evaluateFile(string filePath)
{
    var lexer = Heir.Lexer.FromFile(filePath);
    var tokenStream = lexer.GetTokens();
    var parser = new Heir.Parser(tokenStream);
    var ast = parser.Parse();
    var bytecode = ast.GenerateBytecode();
    var vm = new Heir.VirtualMachine(parser.Diagnostics, bytecode);
    var result = vm.Evaluate();
    Console.WriteLine(result ?? "null");
    Console.WriteLine();

    Console.WriteLine("Diagnostics:");
    foreach (var diagnostic in vm.Diagnostics)
        Console.WriteLine($"{diagnostic.StartLocation} [{diagnostic.Code}] - {diagnostic.Message}");

    return result;
}
