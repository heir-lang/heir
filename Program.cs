var lexer = new Heir.Lexer("foo += bar;");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);