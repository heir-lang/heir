using System.Diagnostics;
using Spectre.Console;
using CommandLine;
using Dumpify;
using Heir.CodeGeneration;
using Heir.Diagnostics;
using StackFrame = Heir.Runtime.StackFrame;

namespace Heir.CLI;

internal class ErrorMarker;

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
                        LoadAndExecuteBytecode(options);
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
                        
                        ExecuteFile(file, options, false); // false is temporary
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
        var stackSize = 0;
        
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            source += input + ";\n";
            var file = new SourceFile(source, "repl", true);
            var (fileExecutionResult, vm) = ExecuteFile(file, options);
            Console.WriteLine($"Execution result: {fileExecutionResult}");
            if (fileExecutionResult is ErrorMarker)
            {
                source = "";
                stackSize = 0;
                continue;
            }

            if (vm == null) continue;
            vm.Stack = new Stack<StackFrame>(vm.Stack.SkipLast(stackSize));
            if (vm.Stack.TryPeek(out var result))
                stackSize++;
                    
            AnsiConsole.MarkupLine(Utility.Repr(result?.Value, true));
        }
    }

    private static (object?, VirtualMachine?) ExecuteFile(SourceFile file, Options options, bool exitAfterFirstError = true)
    {
        ShowInfo(options, file);
        var bytecode = file.GenerateBytecode(); // generate bytecode before timing
        if (file.Diagnostics.HasErrors)
        {
            file.Diagnostics.Write(true, !exitAfterFirstError);
            return (new ErrorMarker(), null);
        }
        
        if (options.BytecodeOutputPath != null) {
            using var fileStream = File.Create(options.BytecodeOutputPath);
            BytecodeSerializer.Serialize(bytecode, fileStream);
            
            return (null, null);
        }
        
        
        var (result, vm, elapsed) = file.Evaluate();
        if (options.ShowBenchmark)
            Console.WriteLine($"Took {elapsed} ms");

        if (file.Diagnostics.HasErrors)
        {
            file.Diagnostics.Write(true, !exitAfterFirstError);
            return (new ErrorMarker(), vm);
        }
        
        return (result, vm);
    }
    
    private static void LoadAndExecuteBytecode(Options options)
    {
        using var fileStream = File.OpenRead(options.FilePath!);
        var deserializedBytecode = BytecodeDeserializer.Deserialize(fileStream);

        var sourceFile = new SourceFile(deserializedBytecode.ToString(), options.FilePath, true);
        var diagnostics = new DiagnosticBag(sourceFile);
        var vm = new VirtualMachine(deserializedBytecode, diagnostics);
        try
        {
            var stopwatch = Stopwatch.StartNew();
            vm.Evaluate();
            stopwatch.Stop();
            if (options.ShowBenchmark)
                Console.WriteLine($"Took {stopwatch.Elapsed.TotalMilliseconds} ms");
        }
        catch (Exception)
        {
            diagnostics.Write();
        }
    }

    private static void ShowInfo(Options options, SourceFile file)
    {
        if (options.ShowTokens)
        {
            Console.WriteLine(file.Tokenize().ToString());
            Console.WriteLine();
        }

        if (options.ShowAST || options.ShowBoundAST)
        {
            var syntaxTree = file.Parse();
            var membersConfig = new MembersConfig
            {
                MemberFilter = valueProvider =>
                    valueProvider.MemberType.ToString() != "Heir.Syntax.Span"
            };

            if (options.ShowAST)
                foreach (var statement in syntaxTree.Statements)
                    statement.Dump(members: membersConfig);

            if (options.ShowBoundAST)
            {
                var boundSyntaxTree = file.Bind().GetBoundSyntaxTree();
                foreach (var statement in boundSyntaxTree.Statements)
                    statement.Dump(members: membersConfig);
            }
        }

        if (options.ShowBytecode)
        {
            Console.WriteLine(file.GenerateBytecode().ToString());
            Console.WriteLine();
        }
    }
}