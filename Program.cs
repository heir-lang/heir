var lexer = new Heir.Lexer("## this is a comment");
var tokenStream = lexer.GetTokens();

Console.WriteLine(tokenStream);