var lexer = new Heir.Lexer("true; false");
var tokenStream = lexer.GetTokens();

Console.WriteLine(tokenStream);