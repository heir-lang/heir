var lexer = new Heir.Lexer("~");
var tokenStream = lexer.GetTokens(true);

Console.WriteLine(tokenStream);

Console.WriteLine("Diagnostics:");
foreach (var diagnostic in lexer.Diagnostics)
    Console.WriteLine($"{diagnostic.StartLocation} [{diagnostic.Code}] - {diagnostic.Message}");