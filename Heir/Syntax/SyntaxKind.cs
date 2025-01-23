namespace Heir.Syntax
{
    public enum SyntaxKind
    {
        /// See <see cref="TriviaKind"/>
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
        EqualsEquals,
        BangEquals,
        Bang,
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
        Question,
        Equals,
        DashRArrow, // ->
        
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

        Ampersand, // bitwise ops
        Pipe,
        Tilde,
        AmpersandEquals,
        PipeEquals,
        TildeEquals,
        LArrowLArrow,
        LArrowLArrowEquals,
        RArrowRArrow,
        RArrowRArrowEquals,

        Identifier,

        IntLiteral, // literals
        FloatLiteral,
        StringLiteral,
        CharLiteral,
        BoolLiteral,
        ObjectLiteral,

        LetKeyword, // keywords
        MutKeyword,
        FnKeyword,
        ReturnKeyword,
        IfKeyword,
        ElseKeyword,
        InterfaceKeyword,
        
        IntKeyword, // type keywords
        FloatKeyword,
        StringKeyword,
        CharKeyword,
        BoolKeyword,
        NoneKeyword
    }
}
