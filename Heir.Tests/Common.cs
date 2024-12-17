using Heir.AST;
using Heir.BoundAST;
using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.Tests
{
    internal static class Common
    {
        public static TokenStream Tokenize(string input)
        {
            var lexer = new Lexer(new("<testing>", input));
            return lexer.GetTokens();
        }

        public static SyntaxTree Parse(string input)
        {
            var tokens = Tokenize(input);
            var parser = new Parser(tokens);
            return parser.Parse();
        }

        public static DiagnosticBag Resolve(string input)
        {
            var syntaxTree = Parse(input);
            var resolver = new Resolver(syntaxTree);
            resolver.Resolve();

            return syntaxTree.Diagnostics;
        }

        public static BoundSyntaxTree Bind(string input)
        {
            var syntaxTree = Parse(input);
            var binder = new Binder(syntaxTree);
            return binder.Bind();
        }

        public static Bytecode GenerateBytecode(string input)
        {
            var syntaxTree = Parse(input);
            var binder = new Binder(syntaxTree);
            binder.Bind();

            var bytecodeGenerator = new BytecodeGenerator(binder, syntaxTree);
            return bytecodeGenerator.GenerateBytecode();
        }

        public static (object?, VirtualMachine) Evaluate(string input)
        {
            var syntaxTree = Parse(input);
            var binder = new Binder(syntaxTree);
            binder.Bind();

            var bytecodeGenerator = new BytecodeGenerator(binder, syntaxTree);
            var bytecode = bytecodeGenerator.GenerateBytecode();
            var vm = new VirtualMachine(binder, bytecode);
            return (vm.Evaluate(), vm);
        }
    }
}
