var lexer = new Heir.Lexer("let x = a;");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);