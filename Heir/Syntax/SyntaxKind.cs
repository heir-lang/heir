﻿namespace Heir.Syntax
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

        Ampersand, // bitops
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

            // type keywords
        IntKeyword,
        FloatKeyword,
        StringKeyword,
        CharKeyword,
        BoolKeyword,
        NoneKeyword
    }
}
