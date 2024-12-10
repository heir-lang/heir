var lexer = Heir.Lexer.FromFile("./Heir/Test.heir");
var tokenStream = lexer.GetTokens();
var parser = new Heir.Parser(tokenStream);
var ast = parser.Parse();

//Console.WriteLine(tokenStream);
//Console.WriteLine();
//ast.Display();
foreach (var instruction in ast.GenerateBytecode())
    Console.WriteLine(instruction.OpCode + ", " + (instruction.Operand?.ToString() ?? "null"));

Console.WriteLine();

Console.WriteLine("Diagnostics:");
foreach (var diagnostic in lexer.Diagnostics)
    Console.WriteLine($"{diagnostic.StartLocation} [{diagnostic.Code}] - {diagnostic.Message}");