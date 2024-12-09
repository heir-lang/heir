namespace Heir.Syntax
{
    internal static class SyntaxFacts
    {
        public static readonly Dictionary<char, int> RadixCodes = new Dictionary<char, int>
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
            { "??=", SyntaxKind.QuestionQuestionEquals }
        });

        public static readonly BiDictionary<string, SyntaxKind> KeywordMap = new(new Dictionary<string, SyntaxKind>
        {
            { "let", SyntaxKind.LetKeyword },
            { "mut", SyntaxKind.MutKeyword }
        });
    }
}
