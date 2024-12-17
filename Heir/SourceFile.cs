using Heir.AST;
using Heir.BoundAST;
using Heir.CodeGeneration;
using Heir.Syntax;
using Spectre.Console;

namespace Heir
{
    public sealed class SourceFile
    {
        public DiagnosticBag Diagnostics { get; }
        public string Source { get; }
        public string Path { get; }
        public bool IsMainFile { get; }

        private TokenStream? _tokens;
        private SyntaxTree? _syntaxTree;
        private Binder? _binder;
        private Bytecode? _bytecode;

        public SourceFile(string source, string? path, bool isMainFile)
        {
            Diagnostics = new(this);
            Source = source;
            Path = path ?? "<anonymous>";
            IsMainFile = isMainFile;
        }

        public static SourceFile FromPath(string path, bool isMainFile = false)
        {
            var source = File.ReadAllText(path);
            return new(source, path, isMainFile);
        }

        public object? Evaluate()
        {
            var bytecode = GenerateBytecode();
            if (Diagnostics.HasErrors)
            {
                WriteDiagnostics();
                return null;
            }

            var vm = new VirtualMachine(bytecode);
            var value = vm.Evaluate();
            if (Diagnostics.HasErrors)
            {
                WriteDiagnostics();
                return null;
            }

            return IsMainFile ? value : null;
        }


        public Bytecode GenerateBytecode()
        {
            if (_bytecode != null)
                return _bytecode;

            var bytecodeGenerator = new BytecodeGenerator(Bind());
            _bytecode = bytecodeGenerator.GenerateBytecode();

            return _bytecode;
        }

        public Binder Bind()
        {
            if (_binder != null)
                return _binder;

            _binder = new Binder(Resolve());
            _binder.Bind();

            return _binder;
        }

        public SyntaxTree Resolve()
        {
            if (_syntaxTree != null)
                return _syntaxTree;

            var syntaxTree = Parse();
            var resolver = new Resolver(syntaxTree);
            resolver.Resolve();

            return syntaxTree;
        }

        public SyntaxTree Parse()
        {
            if (_syntaxTree != null)
                return _syntaxTree;

            var parser = new Parser(Tokenize());
            _syntaxTree = parser.Parse();
            return _syntaxTree;
        }

        public TokenStream Tokenize()
        {
            if (_tokens != null)
                return _tokens;

            var lexer = new Lexer(this);
            _tokens = lexer.GetTokens();
            return _tokens;
        }

        private void WriteDiagnostics() => AnsiConsole.MarkupLine(Diagnostics.ToString(true));
    }
}
