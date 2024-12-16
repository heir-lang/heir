using Heir.AST;
using Heir.BoundAST;
using Heir.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heir.Tests
{
    public static class Common
    {
        public static TokenStream Tokenize(string input)
        {
            var lexer = new Lexer(input, "<testing>");
            return lexer.GetTokens();
        }

        public static SyntaxTree Parse(string input)
        {
            var tokens = Tokenize(input);
            var parser = new Parser(tokens);
            return parser.Parse();
        }

        public static BoundSyntaxTree Bind(string input)
        {
            var syntaxTree = Parse(input);
            var binder = new Binder(syntaxTree);
            return binder.Bind();
        }
    }
}
