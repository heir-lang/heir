using Heir;

var lexer = new Lexer("a += b");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);