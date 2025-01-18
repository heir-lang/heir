using Spectre.Console;
using CommandLine;
using Heir;
using Heir.AST;

public static class Program
{
    public class Options
    {
        [Option('t', "tokens", Required = false, HelpText = "Output the emitted tokens.")]
        public bool ShowTokens { get; set; }
        [Option('a', "ast", Required = false, HelpText = "Output a visualization of the AST.")]
        public bool ShowAST { get; set; }
        [Option('b', "bound-ast", Required = false, HelpText = "Output a visualization of the bound AST.")]
        public bool ShowBoundAST { get; set; }
        [Option('c', "bytecode", Required = false, HelpText = "Output the emitted bytecode.")]
        public bool ShowBytecode { get; set; }
        
        [Value(0, MetaName = "file-path", HelpText = "Path to the file to be executed with Heir.")]
        public string? FilePath { get; set; }
    }

    public static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                if (options.FilePath != null)
                    ExecuteFile(options);
                else
                    StartRepl(options);
            });
    }

    private static void StartRepl(Options options)
    {
        Console.WriteLine("Welcome to the Heir REPL!");
        // AnsiConsole.MarkupLine(Utility.Repr(result, true));
    }

    private static void ExecuteFile(Options options)
    {
        var program = new HeirProgram();
        SourceFile file;
        try
        {
            file = SourceFile.FromPath(options.FilePath!, isMainFile: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return;
        }

        ShowInternals(options, file);
        program.LoadFile(file);
        program.Evaluate();
    }

    private static void ShowInternals(Options options, SourceFile file)
    {
        if (options.ShowTokens)
        {
            Console.WriteLine(file.Tokenize().WithoutTrivia().ToString());
            Console.WriteLine();
        }

        if (options.ShowAST || options.ShowBoundAST)
        {
            var syntaxTree = file.Parse();
            if (options.ShowAST)
            {
                syntaxTree.Display();
                Console.WriteLine();
            }

            if (options.ShowBoundAST)
            {
                file.Bind().GetBoundNode(syntaxTree).Display();
                Console.WriteLine();
            }
        }

        if (options.ShowBytecode)
        {
            Console.WriteLine(file.GenerateBytecode().ToString());
            Console.WriteLine();
        }
    }
}