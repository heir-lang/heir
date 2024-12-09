namespace Heir.Syntax
{
    public enum SyntaxKind
    {
        Trivia,

        LParen, // brackets
        RParen,
        LBracket,
        RBracket,
        LBrace,
        RBrace,

        LT, // equality
        GT,
        LTE,
        GTE,
        Equals,
        EqualsEquals,
        BangEquals,
        Bang,
        Question,
        AmpersandAmpersand,
        PipePipe,
        AmpersandAmpersandEquals,
        PipePipeEquals,

        QuestionQuestion,
        QuestionQuestionEquals,
        Dot,
        Colon,
        ColonColon,
        Comma,
        
        Plus, // arithmetic
        PlusEquals,
        PlusPlus,
        Minus,
        MinusEquals,
        MinusMinus,
        Star,
        StarEquals,
        Slash,
        SlashEquals,
        SlashSlash,
        SlashSlashEquals,
        Percent,
        PercentEquals,
        Carat,
        CaratEquals,

        Ampersand, // bitops
        Pipe,
        Tilde,
        AmpersandEquals,
        PipeEquals,
        TildeEquals,

        Identifier,

        IntLiteral, // literals
        FloatLiteral,
        StringLiteral,
        CharLiteral,
        BoolLiteral,

        LetKeyword, // keywords
        MutKeyword,

            // type keywords
        IntKeyword,
        FloatKeyword,
        StringKeyword,
        CharKeyword,
        BoolKeyword,
        NoneKeyword
    }
}
