var lexer = new Heir.Lexer("(2 * 4) + 6");
var tokenStream = lexer.GetTokens(true);
var parser = new Heir.Parser(tokenStream);
var ast = parser.ParseExpression();

ast.Display();
Console.WriteLine();
Console.WriteLine();

Console.WriteLine("Diagnostics:");
foreach (var diagnostic in lexer.Diagnostics)
    Console.WriteLine($"{diagnostic.StartLocation} [{diagnostic.Code}] - {diagnostic.Message}");