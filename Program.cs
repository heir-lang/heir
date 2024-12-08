var lexer = new Heir.Lexer("let x = 123.456;");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);