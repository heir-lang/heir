using Heir.AST;
using Heir.BoundAST;
using Heir.CodeGeneration;
using Heir.Diagnostics;
using Heir.Syntax;

namespace Heir.Tests;

internal static class Common
{
    public static TokenStream Tokenize(string input)
    {
        var sourceFile = CreateSourceFile(input);
        return sourceFile.Tokenize();
    }

    public static SyntaxTree Parse(string input)
    {
        var sourceFile = CreateSourceFile(input);
        return sourceFile.Parse(false);
    }

    public static DiagnosticBag Resolve(string input)
    {
        var sourceFile = CreateSourceFile(input);
        sourceFile.Resolve(false);

        return sourceFile.Diagnostics;
    }

    public static Binder Bind(string input)
    {
        var sourceFile = CreateSourceFile(input);
        return sourceFile.Bind();
    }

    public static DiagnosticBag TypeCheck(string input)
    {
        var sourceFile = CreateSourceFile(input);
        sourceFile.TypeCheck();

        return sourceFile.Diagnostics;
    }

    public static Bytecode GenerateBytecode(string input)
    {
        var sourceFile = CreateSourceFile(input);
        return sourceFile.GenerateBytecode();
    }

    public static (object?, VirtualMachine) Evaluate(string input)
    {
        var sourceFile = CreateSourceFile(input);
        return sourceFile.Evaluate(false);
    }

    private static SourceFile CreateSourceFile(string input) => new(input, "<testing>", true);

}
