namespace Heir.Syntax
{
    public enum SyntaxKind
    {
        EOF,
        Trivia,

        LParen,
        RParen,
        LBracket,
        RBracket,
        LBrace,
        RBrace,
        LT,
        GT,
        LTE,
        GTE,
        Equals,
        EqualsEquals,
        BangEquals,
        Bang,
        Dot,
        Question,
        QuestionQuestion,

        Plus,
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
        
        Identifier,
        IntLiteral,
        FloatLiteral,
        StringLiteral,
        BoolLiteral,
        NoneLiteral
    }
}
