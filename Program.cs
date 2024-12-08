var lexer = new Heir.Lexer("\"abcd\"");
var tokenStream = lexer.GetTokens(true);

Console.WriteLine(tokenStream);