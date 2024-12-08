using Heir;

var lexer = new Lexer("++ += +");
var tokenStream = lexer.GetTokens();

foreach (var token in tokenStream)
    Console.WriteLine(token);