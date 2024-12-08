var lexer = new Heir.Lexer("'c'");
var tokenStream = lexer.GetTokens(true);

Console.WriteLine(tokenStream);