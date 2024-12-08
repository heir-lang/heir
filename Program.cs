var lexer = new Heir.Lexer("let x = 0o621;");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);