using Heir.Types;

namespace Heir.Syntax
{
    public static class SyntaxFacts
    {
        public static readonly BiDictionary<string, SyntaxKind> KeywordMap = new(new Dictionary<string, SyntaxKind>
        {
            { "let", SyntaxKind.LetKeyword },
            { "mut", SyntaxKind.MutKeyword },
            { "fn", SyntaxKind.FnKeyword },
            { "return", SyntaxKind.ReturnKeyword },
            { "if", SyntaxKind.IfKeyword },
            { "else", SyntaxKind.ElseKeyword },
            { "while", SyntaxKind.WhileKeyword },
            { "interface", SyntaxKind.InterfaceKeyword },
            { "nameof", SyntaxKind.NameofKeyword },

            { "int", SyntaxKind.IntKeyword },
            { "float", SyntaxKind.FloatKeyword },
            { "string", SyntaxKind.StringKeyword },
            { "char", SyntaxKind.CharKeyword },
            { "bool", SyntaxKind.BoolKeyword },
            { "none", SyntaxKind.NoneKeyword }
        });
        
        public static readonly HashSet<SyntaxKind> TypeSyntaxes =
        [
            SyntaxKind.Identifier,
            SyntaxKind.StringKeyword,
            SyntaxKind.CharKeyword,
            SyntaxKind.IntKeyword,
            SyntaxKind.FloatKeyword,
            SyntaxKind.BoolKeyword,
            SyntaxKind.NoneKeyword
        ];

        public static readonly HashSet<SyntaxKind> BinaryCompoundAssignmentOperators =
        [
            SyntaxKind.PlusEquals,
            SyntaxKind.MinusEquals,
            SyntaxKind.StarEquals,
            SyntaxKind.SlashEquals,
            SyntaxKind.SlashSlashEquals,
            SyntaxKind.PercentEquals,
            SyntaxKind.CaratEquals,
            SyntaxKind.AmpersandEquals,
            SyntaxKind.PipeEquals,
            SyntaxKind.TildeEquals,
            SyntaxKind.AmpersandAmpersandEquals,
            SyntaxKind.PipePipeEquals
        ];

        public static readonly HashSet<SyntaxKind> NumberOperators =
        [
            SyntaxKind.Plus,
            SyntaxKind.Minus,
            SyntaxKind.Star,
            SyntaxKind.Slash,
            SyntaxKind.SlashSlash,
            SyntaxKind.Percent,
            SyntaxKind.Carat,
            SyntaxKind.Ampersand,
            SyntaxKind.Pipe,
            SyntaxKind.Tilde,
            SyntaxKind.PlusEquals,
            SyntaxKind.MinusEquals,
            SyntaxKind.StarEquals,
            SyntaxKind.SlashEquals,
            SyntaxKind.SlashSlashEquals,
            SyntaxKind.PercentEquals,
            SyntaxKind.CaratEquals,
            SyntaxKind.AmpersandEquals,
            SyntaxKind.PipeEquals,
            SyntaxKind.TildeEquals,
            SyntaxKind.MinusMinus,
            SyntaxKind.PlusPlus,
        ];

        public static readonly Dictionary<SyntaxKind, PrimitiveType> PrimitiveTypeMap = new()
        {
            { SyntaxKind.StringKeyword, PrimitiveType.String },
            { SyntaxKind.CharKeyword, PrimitiveType.Char },
            { SyntaxKind.IntKeyword, PrimitiveType.Int },
            { SyntaxKind.FloatKeyword, PrimitiveType.Float },
            { SyntaxKind.BoolKeyword, PrimitiveType.Bool },
            { SyntaxKind.NoneKeyword, PrimitiveType.None }
        };

        public static readonly Dictionary<char, int> RadixCodes = new()
        {
            { 'b', 2 },
            { 'o', 8 },
            { 'x', 16 }
        };

        public static readonly BiDictionary<string, SyntaxKind> OperatorMap = new(new Dictionary<string, SyntaxKind>
        {
            { "+", SyntaxKind.Plus },
            { "+=", SyntaxKind.PlusEquals },
            { "++", SyntaxKind.PlusPlus },
            { "-", SyntaxKind.Minus },
            { "-=", SyntaxKind.MinusEquals },
            { "--", SyntaxKind.MinusMinus },
            { "*", SyntaxKind.Star },
            { "*=", SyntaxKind.StarEquals },
            { "/", SyntaxKind.Slash },
            { "/=", SyntaxKind.SlashEquals },
            { "//", SyntaxKind.SlashSlash },
            { "//=", SyntaxKind.SlashSlashEquals },
            { "%", SyntaxKind.Percent },
            { "%=", SyntaxKind.PercentEquals },
            { "^", SyntaxKind.Carat },
            { "^=", SyntaxKind.CaratEquals },
            { "??", SyntaxKind.QuestionQuestion },
            { "??=", SyntaxKind.QuestionQuestionEquals },
            { "&", SyntaxKind.Ampersand },
            { "|", SyntaxKind.Pipe },
            { "~", SyntaxKind.Tilde },
            { "~=", SyntaxKind.TildeEquals },
            { "&&", SyntaxKind.AmpersandAmpersand },
            { "&&=", SyntaxKind.AmpersandAmpersandEquals },
            { "||", SyntaxKind.PipePipe },
            { "||=", SyntaxKind.PipePipeEquals },

            { "<", SyntaxKind.LT },
            { "<=", SyntaxKind.LTE },
            { ">", SyntaxKind.GT },
            { ">=", SyntaxKind.GTE },
            { "!", SyntaxKind.Bang },
            { "!=", SyntaxKind.BangEquals },
            { "==", SyntaxKind.EqualsEquals },

            { "?", SyntaxKind.Question },
            { "=", SyntaxKind.Equals },
        });
    }
}
