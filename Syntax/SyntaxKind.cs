namespace Heir.Syntax
{
    public enum SyntaxKind
    {
        EOF, 

        LParen,
        RParen,
        LBracket,
        RBracket,
        LBrace,
        RBrace,
        Equals,
        EqualsEquals,
        BangEquals,
        Bang,
        Dot,
        Question,
        QuestionQuestion,

        Plus,
        PlusEqual,
        PlusPlus,
        Minus,
        MinusEqual,
        MinusMinus,
        Star,
        StarEqual,
        Slash,
        SlashEqual,
        SlashSlash,
        SlashSlashEqual,
        Percent,
        PercentEqual,
        Carat,
        CaratEqual,
        
        Identifier,
        IntLiteral,
        FloatLiteral,
        StringLiteral,
        BoolLiteral,
        NoneLiteral
    }
}
