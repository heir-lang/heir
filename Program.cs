var lexer = new Heir.Lexer("none; non; tru; fals; false; true");
var tokenStream = lexer.GetTokens(true);

Console.WriteLine(tokenStream);