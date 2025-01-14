using Spectre.Console;
using Heir.Syntax;
using Heir.AST;
using Heir.CodeGeneration;

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
            Diagnostics = new DiagnosticBag(this);
            Source = source;
            Path = path ?? "<anonymous>";
            IsMainFile = isMainFile;
        }

        public static SourceFile FromPath(string path, bool isMainFile = false)
        {
            var source = File.ReadAllText(path);
            return new SourceFile(source, path, isMainFile);
        }

        public (object?, VirtualMachine) Evaluate()
        {
            var bytecode = GenerateBytecode();
            var vm = new VirtualMachine(bytecode);
            if (Diagnostics.Count > 0)
            {
                WriteDiagnostics();
                return (null, vm);
            }

            try
            {
                var value = vm.Evaluate();
                if (Diagnostics.Count <= 0)
                    return (IsMainFile ? value : null, vm);
            }
            catch (Exception e)
            {
                WriteDiagnostics();
            }
            
            return (null, vm);
        }

        public Bytecode GenerateBytecode()
        {
            if (_bytecode != null)
                return _bytecode;

            var bytecodeGenerator = new BytecodeGenerator(Diagnostics, TypeCheck());
            _bytecode = bytecodeGenerator.GenerateBytecode();

            return _bytecode;
        }

        public Binder TypeCheck()
        {
            var binder = _binder ?? Bind();
            var boundTree = binder.GetBoundSyntaxTree();
            var typeChecker = new TypeChecker(Diagnostics, boundTree);
            typeChecker.Check();

            return binder;
        }

        public Binder Bind(bool resolve = true)
        {
            if (_binder != null)
                return _binder;

            _binder = new Binder(Diagnostics, Resolve());
            _binder.Bind();

            return _binder;
        }

        public SyntaxTree Resolve()
        {
            var syntaxTree = _syntaxTree ?? Parse();
            var resolver = new Resolver(Diagnostics, syntaxTree);
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
