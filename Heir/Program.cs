using Spectre.Console;
using Heir;

var program = new HeirProgram();
var testFile = SourceFile.FromPath("./Test.heir", isMainFile: true);
// Console.WriteLine(testFile.Tokenize().WithoutTrivia().ToString());
// Console.WriteLine();
// var syntaxTree = testFile.Parse();
// syntaxTree.Display();
// Console.WriteLine();
// testFile.Bind().GetBoundNode(syntaxTree).Display();
// Console.WriteLine();
// Console.WriteLine(testFile.GenerateBytecode().ToString());
// Console.WriteLine();

program.LoadFile(testFile);
var result = program.Evaluate();
AnsiConsole.MarkupLine(Utility.Repr(result, true));
