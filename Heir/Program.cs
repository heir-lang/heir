var lexer = Heir.Lexer.FromFile("./Heir/Test.heir");
var tokenStream = lexer.GetTokens();
var parser = new Heir.Parser(tokenStream);
var ast = parser.Parse();

//Console.WriteLine(tokenStream);
//Console.WriteLine();
//ast.Display();
var bytecode = ast.GenerateBytecode();
//foreach (var instruction in bytecode)
//    Console.WriteLine(instruction.OpCode + ", " + (instruction.Operand?.ToString() ?? "null"));

//Console.WriteLine();

var vm = new Heir.VirtualMachine(parser.Diagnostics, bytecode);
Console.WriteLine(vm.Evaluate() ?? "null");
Console.WriteLine();

Console.WriteLine("Diagnostics:");
foreach (var diagnostic in lexer.Diagnostics)
    Console.WriteLine($"{diagnostic.StartLocation} [{diagnostic.Code}] - {diagnostic.Message}");