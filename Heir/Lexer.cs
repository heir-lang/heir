using Heir.Syntax;

namespace Heir;

public sealed class Lexer(SourceFile sourceFile)
{
    private readonly DiagnosticBag _diagnostics = sourceFile.Diagnostics;
    private readonly List<Token> _tokens = [];
    private string _currentLexeme = "";
    private int _position;
    private int _line = 1;
    private int _column;
        
    private Location _currentLocation => new(sourceFile.Path, _line, _column, _position);
    private bool _isFinished => _position >= sourceFile.Source.Length;
    private char? _current => Peek(0);
    private char? _previous => Peek(-1);

    public TokenStream GetTokens()
    {
        while (!_isFinished)
        {
            var location = _currentLocation;
            var token = Lex();
            if (token == null)
            {
                _diagnostics.Error(DiagnosticCode.H001, $"Unexpected character \"{_previous}\"", location, _currentLocation);
                continue;
            }

            _currentLexeme = "";
            _tokens.Add(token);
        }

        _tokens.Add(TokenFactory.Trivia(TriviaKind.EOF, "", _currentLocation, _currentLocation));
        return new TokenStream(_diagnostics, _tokens.ToArray());
    }

    private Token? Lex()
    {
        if (_isFinished) return null;
        var startLocation = _currentLocation;
        var current = (char)_current!;

        Advance();
        switch (current)
        {
            case '+':
            {
                var syntaxKind = SyntaxKind.Plus;
                if (Match('+'))
                    syntaxKind = SyntaxKind.PlusPlus;
                else if (Match('='))
                    syntaxKind = SyntaxKind.PlusEquals;

                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '-':
            {
                var syntaxKind = SyntaxKind.Minus;
                if (Match('-'))
                    syntaxKind = SyntaxKind.MinusMinus;
                else if (Match('='))
                    syntaxKind = SyntaxKind.MinusEquals;
                else if (Match('>'))
                    syntaxKind = SyntaxKind.DashRArrow;
                    
                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '*':
            {
                var syntaxKind = SyntaxKind.Star;
                if (Match('='))
                    syntaxKind = SyntaxKind.StarEquals;

                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '/':
            {
                var syntaxKind = SyntaxKind.Slash;
                if (Match('/'))
                {
                    syntaxKind = SyntaxKind.SlashSlash;
                    if (Match('='))
                        syntaxKind = SyntaxKind.SlashSlashEquals;

                    return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
                }
                        
                if (Match('='))
                    syntaxKind = SyntaxKind.SlashEquals;

                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '%':
            {
                var syntaxKind = SyntaxKind.Percent;
                if (Match('='))
                    syntaxKind = SyntaxKind.PercentEquals;

                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            // TODO: refactor the rest like above
            case '^':
            {
                if (Match('='))
                    return TokenFactory.Operator(SyntaxKind.CaratEquals, _currentLexeme, startLocation, _currentLocation);

                return TokenFactory.Operator(SyntaxKind.Carat, _currentLexeme, startLocation, _currentLocation);
            }
            case '~':
            {
                if (Match('='))
                    return TokenFactory.Operator(SyntaxKind.TildeEquals, _currentLexeme, startLocation, _currentLocation);

                return TokenFactory.Operator(SyntaxKind.Tilde, _currentLexeme, startLocation, _currentLocation);
            }

            case '=':
            {
                if (Match('='))
                    return TokenFactory.Operator(SyntaxKind.EqualsEquals, _currentLexeme, startLocation, _currentLocation);

                return TokenFactory.Operator(SyntaxKind.Equals, _currentLexeme, startLocation, _currentLocation);
            }
            case '!':
            {
                if (Match('='))
                    return TokenFactory.Operator(SyntaxKind.BangEquals, _currentLexeme, startLocation, _currentLocation);

                return TokenFactory.Operator(SyntaxKind.Bang, _currentLexeme, startLocation, _currentLocation);
            }
            case '?':
            {
                if (Match('?'))
                {
                    var syntaxKind = Match('=')
                        ? SyntaxKind.QuestionQuestionEquals
                        : SyntaxKind.QuestionQuestion;
                            
                    return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
                }

                return TokenFactory.Operator(SyntaxKind.Question, _currentLexeme, startLocation, _currentLocation);
            }
            case '&':
            {
                if (Match('&'))
                {
                    var syntaxKind = Match('=')
                        ? SyntaxKind.AmpersandAmpersandEquals
                        : SyntaxKind.AmpersandAmpersand;
                            
                    return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
                }
                        
                if (Match('='))
                    return TokenFactory.Operator(SyntaxKind.AmpersandEquals, _currentLexeme, startLocation, _currentLocation);

                return TokenFactory.Operator(SyntaxKind.Ampersand, _currentLexeme, startLocation, _currentLocation);
            }
            case '|':
            {
                if (Match('|'))
                {
                    var syntaxKind = Match('=') ? SyntaxKind.PipePipeEquals : SyntaxKind.PipePipe;
                    return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
                }
                        
                if (Match('='))
                    return TokenFactory.Operator(SyntaxKind.PipeEquals, _currentLexeme, startLocation, _currentLocation);

                return TokenFactory.Operator(SyntaxKind.Pipe, _currentLexeme, startLocation, _currentLocation);
            }

            case ':':
            {
                var syntaxKind = Match(':') ? SyntaxKind.ColonColon : SyntaxKind.Colon;
                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '.':
                return TokenFactory.Operator(SyntaxKind.Dot, _currentLexeme, startLocation, _currentLocation);
            case ',':
                return TokenFactory.Operator(SyntaxKind.Comma, _currentLexeme, startLocation, _currentLocation);

            case '(':
                return TokenFactory.Operator(SyntaxKind.LParen, _currentLexeme, startLocation, _currentLocation);
            case ')':
                return TokenFactory.Operator(SyntaxKind.RParen, _currentLexeme, startLocation, _currentLocation);
            case '[':
                return TokenFactory.Operator(SyntaxKind.LBracket, _currentLexeme, startLocation, _currentLocation);
            case ']':
                return TokenFactory.Operator(SyntaxKind.RBracket, _currentLexeme, startLocation, _currentLocation);
            case '{':
                return TokenFactory.Operator(SyntaxKind.LBrace, _currentLexeme, startLocation, _currentLocation);
            case '}':
                return TokenFactory.Operator(SyntaxKind.RBrace, _currentLexeme, startLocation, _currentLocation);
            case '<':
            {
                SyntaxKind syntaxKind;
                if (Match('='))
                    syntaxKind = SyntaxKind.LTE;
                else if (Match('<'))
                    syntaxKind = Match('=') ? SyntaxKind.LArrowLArrowEquals : SyntaxKind.LArrowLArrow;
                else
                    syntaxKind = SyntaxKind.LT;
                    
                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '>':
            {
                var syntaxKind = SyntaxKind.GT;
                if (Match('='))
                    syntaxKind = SyntaxKind.GTE;
                else if (Match('>'))
                    syntaxKind = Match('=') ? SyntaxKind.RArrowRArrowEquals : SyntaxKind.RArrowRArrow;
                    
                return TokenFactory.Operator(syntaxKind, _currentLexeme, startLocation, _currentLocation);
            }
            case '"':
                return ReadString(startLocation);
            case '\'':
                return ReadCharacter(startLocation);

            case '#':
                return Match('#') ? SkipComment(startLocation) : null;
            case ';':
                return SkipSemicolons(startLocation);

            case '\r':
                return Lex();
            case '\n':
                return SkipNewLines(startLocation);

            default:
            {
                if (char.IsLetter(current))
                {
                    if (MatchLexeme("true") || MatchLexeme("false"))
                        return TokenFactory.BoolLiteral(_currentLexeme, startLocation, _currentLocation);

                    var identifier = ReadIdentifier(startLocation);
                    if (!SyntaxFacts.KeywordMap.Contains(_currentLexeme))
                        return identifier;
                            
                    var keywordSyntax = SyntaxFacts.KeywordMap.GetValue(_currentLexeme);
                    return TokenFactory.Keyword(keywordSyntax, startLocation, _currentLocation);
                }
                        
                if (char.IsDigit(current))
                    return ReadNumber(startLocation);
                        
                if (char.IsWhiteSpace(current))
                    return SkipWhitespace(startLocation);

                return null;
            }
        }
    }

    private Token ReadCharacter(Location location)
    {
        Advance();
        if (_current != '\'')
            _diagnostics.Error(DiagnosticCode.H002B, $"Unterminated character", location, _currentLocation);

        Advance();
        return TokenFactory.CharLiteral(_currentLexeme, location, _currentLocation);
    }

    private Token ReadString(Location location)
    {
        while (!_isFinished && _current != '"' && _current != '\n')
            Advance();

        if (_current != '"')
            _diagnostics.Error(DiagnosticCode.H002, $"Unterminated string", location, _currentLocation);

        Advance();
        return TokenFactory.StringLiteral(_currentLexeme, location, _currentLocation);
    }

    private Token ReadNumber(Location location)
    {
        if (_previous == '0')
        {
            char code = (char)_current!;
            if (SyntaxFacts.RadixCodes.ContainsKey(code))
            {
                int radix = SyntaxFacts.RadixCodes[code];
                return ReadNonDecimalNumber(location, radix);
            }
        }

        var decimalUsed = false;
        while (char.IsDigit((char)_current!) || _current == '.') // fuck you C#
        {
            if (_current == '.')
            {
                if (decimalUsed)
                {
                    _diagnostics.Error(DiagnosticCode.H003, "Malformed number", location, _currentLocation);
                    break;
                }
                decimalUsed = true;
            }

            Advance();
        }

        // TODO: handle conversion errors
        return decimalUsed ?
            TokenFactory.FloatLiteral(_currentLexeme, location, _currentLocation)
            : TokenFactory.IntLiteral(_currentLexeme, location, _currentLocation);
    }

    private Token ReadNonDecimalNumber(Location location, int radix)
    {
        Advance();
        while (char.IsLetterOrDigit((char)_current!)) // fuck you C#
            Advance();

        return TokenFactory.IntLiteral(_currentLexeme, location, _currentLocation, radix);
    }

    private Token ReadIdentifier(Location location)
    {
        while (char.IsLetterOrDigit((char)_current!) || _current == '_') // fuck you C#
            Advance();

        return TokenFactory.Identifier(_currentLexeme, location, _currentLocation);
    }

    private Token SkipWhitespace(Location location)
    {
        while (char.IsWhiteSpace((char)_current!) && _current != '\n') // fuck you C#
            Advance();

        return TokenFactory.Trivia(TriviaKind.Whitespace, _currentLexeme, location, _currentLocation);
    }

    private Token SkipNewLines(Location location)
    {
        _line++;
        while (_current == '\n')
        {
            _position++; // Advance() but w/o adding to _column for performance reasons
            _line++;
        }

        _column = 0;
        return TokenFactory.Trivia(TriviaKind.Newlines, _currentLexeme, location, _currentLocation);
    }

    private Token SkipSemicolons(Location location)
    {
        while (_current == ';')
            Advance();

        return TokenFactory.Trivia(TriviaKind.Semicolons, _currentLexeme, location, _currentLocation);
    }

    private Token SkipComment(Location location)
    {
        while (!_isFinished && _current != '\n')
            Advance();

        return TokenFactory.Trivia(TriviaKind.Comment, _currentLexeme, location, _currentLocation);
    }

    private bool MatchLexeme(string lexeme)
    {
        var characters = lexeme.ToCharArray().ToList();
        var isMatch = _previous == characters.First();
        if (!isMatch) return false;

        foreach (var character in characters.Skip(1))
        {
            if (_current != character)
                return false;

            Advance();
        }                   

        return isMatch;
    }

    private bool Match(char character)
    {
        var isMatch = _current == character;
        if (isMatch)
            Advance();

        return isMatch;
    }

    private void Advance()
    {
        _currentLexeme += _current;
        _position++;
        _column++;
    }

    private char? Peek(int offset = 1)
    {
        return sourceFile.Source
            .ToCharArray()
            .ElementAtOrDefault(_position + offset);
    }
}