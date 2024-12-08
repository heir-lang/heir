var lexer = new Heir.Lexer("true; false");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);