using System.Diagnostics;
using CommandLine;
using Heir.CodeGeneration;
using Spectre.Console;

namespace Heir.CLI;

public static class Program
{
    private class Options
    {
        [Option('t', "tokens", Required = false, HelpText = "Output the emitted tokens.")]
        public bool ShowTokens { get; set; }
        [Option('a', "ast", Required = false, HelpText = "Output a visualization of the AST.")]
        public bool ShowAST { get; set; }
        [Option('b', "bound-ast", Required = false, HelpText = "Output a visualization of the bound AST.")]
        public bool ShowBoundAST { get; set; }
        [Option('c', "bytecode", Required = false, HelpText = "Output the emitted bytecode.")]
        public bool ShowBytecode { get; set; }
        [Option('e', "benchmark", Required = false, HelpText = "Output the amount of time taken to evaluate the program.")]
        public bool ShowBenchmark { get; set; }
        
        [Option("load-bytecode", Required = false, HelpText = "Execute a .bin file containing bytecode using the HVM.")]
        public bool LoadBytecode { get; set; }
        [Option('o', "save-bytecode", Required = false, HelpText = "Save bytecode to this file path instead of executing it")]
        public string? BytecodeOutputPath { get; set; }
        
        [Value(0, MetaName = "file-path", HelpText = "Path to the file to be executed with Heir.")]
        public string? FilePath { get; set; }
    }

    public static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                if (options.LoadBytecode)
                {
                    if (options.FilePath == null)
                    {
                        Console.WriteLine("Failed to call HVM: No file path provided");
                        Environment.Exit(1);
                        return;
                    }
                    if (!options.FilePath.EndsWith(".bin"))
                    {
                        Console.WriteLine($"Failed to call HVM: Provided invalid bytecode file type, got {Path.GetExtension(options.FilePath)}");
                        Environment.Exit(1);
                        return;
                    }
                }
                
                if (options.FilePath != null)
                {
                    if (options.LoadBytecode)
                    {
                        using var fileStream = File.OpenRead(options.FilePath);
                        var deserializedBytecode = BytecodeDeserializer.Deserialize(fileStream);

                        var sourceFile = new SourceFile(deserializedBytecode.ToString(), options.FilePath, true);
                        var vm = new VirtualMachine(deserializedBytecode, new DiagnosticBag(sourceFile));
                        var stopwatch = Stopwatch.StartNew();
                        vm.Evaluate();
                        stopwatch.Stop();
        
                        if (options.ShowBenchmark)
                            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        SourceFile file;
                        try
                        {
                            file = SourceFile.FromPath(options.FilePath, isMainFile: true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading file: {ex.Message}");
                            Environment.Exit(1);
                            return;
                        }
                        ExecuteFile(file, options);
                    }
                }
                else
                    StartRepl(options);
            });
    }

    private static void StartRepl(Options options)
    {
        Console.WriteLine("Welcome to the Heir REPL!");
        var source = "";
        
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            source += input + "\n";
            
            var file = new SourceFile(source, "repl", true);
            var result = ExecuteFile(file, options);
            AnsiConsole.MarkupLine(Utility.Repr(result, true));
        }
    }

    private static object? ExecuteFile(SourceFile file, Options options)
    {
        ShowInfo(options, file);
        var bytecode = file.GenerateBytecode(); // generate bytecode before timing
        if (options.BytecodeOutputPath != null) {
            using var fileStream = File.Create(options.BytecodeOutputPath);
            BytecodeSerializer.Serialize(bytecode, fileStream);
            
            return null;
        }
        
        var stopwatch = Stopwatch.StartNew();
        var (result, _) = file.Evaluate();
        stopwatch.Stop();
    
        if (options.ShowBenchmark)
            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms");
        
        return result;
    }

    private static void ShowInfo(Options options, SourceFile file)
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