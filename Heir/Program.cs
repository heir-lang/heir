var lexer = Heir.Lexer.FromFile("./Heir/Test.heir");
var tokenStream = lexer.GetTokens(true);
var parser = new Heir.Parser(tokenStream);
var ast = parser.ParseExpression();

Console.WriteLine(tokenStream);
Console.WriteLine();
ast.Display();
Console.WriteLine();
Console.WriteLine();

Console.WriteLine("Diagnostics:");
foreach (var diagnostic in lexer.Diagnostics)
    Console.WriteLine($"{diagnostic.StartLocation} [{diagnostic.Code}] - {diagnostic.Message}");