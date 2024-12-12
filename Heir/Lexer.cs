using Heir.Syntax;

namespace Heir
{
    public sealed class Lexer(string source, string fileName = "<anonymous>")
    {
        public string Source { get; } = source;
        public DiagnosticBag Diagnostics { get; } = new();

        private readonly string _fileName = fileName;
        private List<Token> _tokens = [];
        private string _currentLexeme = "";
        private int _position = 0;
        private int _line = 1;
        private int _column = 0;
        private Location _currentLocation
        {
            get => new Location(_fileName, _line, _column, _position);
        }
        private bool _isFinished
        {
            get => _position >= Source.Length;
        }
        private char? _current
        {
            get => Peek(0);
        }
        private char? _previous
        {
            get => Peek(-1);
        }

        public static Lexer FromFile(string path)
        {
            string fileName = Path.GetFileName(path);
            string contents = File.ReadAllText(path);
            return new Lexer(contents, fileName);
        }

        public TokenStream GetTokens()
        {
            while (!_isFinished)
            {
                var location = _currentLocation;
                var token = Lex();
                if (token == null)
                {
                    Diagnostics.Error("H001", $"Unexpected character \"{_previous}\"", location, _currentLocation);
                    continue;
                }

                _currentLexeme = "";
                _tokens.Add(token);
            }

            _tokens.Add(TokenFactory.Trivia(TriviaKind.EOF, "", _currentLocation, _currentLocation));
            return new TokenStream(Diagnostics, _tokens.ToArray());
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
                        if (Match('+'))
                            return TokenFactory.Operator(SyntaxKind.PlusPlus, _currentLexeme, startLocation, _currentLocation);
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PlusEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Plus, _currentLexeme, startLocation, _currentLocation);
                    }
                case '-':
                    {
                        if (Match('-'))
                            return TokenFactory.Operator(SyntaxKind.MinusMinus, _currentLexeme, startLocation, _currentLocation);
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.MinusEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Minus, _currentLexeme, startLocation, _currentLocation);
                    }
                case '*':
                    {
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.StarEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Star, _currentLexeme, startLocation, _currentLocation);
                    }
                case '/':
                    {
                        if (Match('/'))
                        {
                            if (Match('='))
                                return TokenFactory.Operator(SyntaxKind.SlashSlashEquals, _currentLexeme, startLocation, _currentLocation);

                            return TokenFactory.Operator(SyntaxKind.SlashSlash, _currentLexeme, startLocation, _currentLocation);
                        }
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.SlashEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Slash, _currentLexeme, startLocation, _currentLocation);
                    }
                case '%':
                    {
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PercentEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Percent, _currentLexeme, startLocation, _currentLocation);
                    }
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
                            if (Match('='))
                                return TokenFactory.Operator(SyntaxKind.QuestionQuestionEquals, _currentLexeme, startLocation, _currentLocation);

                            return TokenFactory.Operator(SyntaxKind.QuestionQuestion, _currentLexeme, startLocation, _currentLocation);
                        }

                        return TokenFactory.Operator(SyntaxKind.Question, _currentLexeme, startLocation, _currentLocation);
                    }
                case '&':
                    {
                        if (Match('&'))
                        {
                            if (Match('='))
                                return TokenFactory.Operator(SyntaxKind.AmpersandAmpersandEquals, _currentLexeme, startLocation, _currentLocation);

                            return TokenFactory.Operator(SyntaxKind.AmpersandAmpersand, _currentLexeme, startLocation, _currentLocation);
                        }
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.AmpersandEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Ampersand, _currentLexeme, startLocation, _currentLocation);
                    }
                case '|':
                    {
                        if (Match('|'))
                        {
                            if (Match('='))
                                return TokenFactory.Operator(SyntaxKind.PipePipeEquals, _currentLexeme, startLocation, _currentLocation);

                            return TokenFactory.Operator(SyntaxKind.PipePipe, _currentLexeme, startLocation, _currentLocation);
                        }
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PipeEquals, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Pipe, _currentLexeme, startLocation, _currentLocation);
                    }

                case ':':
                    {
                        if (Match(':'))
                            return TokenFactory.Operator(SyntaxKind.ColonColon, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.Colon, _currentLexeme, startLocation, _currentLocation);
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
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.LTE, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.LT, _currentLexeme, startLocation, _currentLocation);
                    }
                case '>':
                    {
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.GTE, _currentLexeme, startLocation, _currentLocation);

                        return TokenFactory.Operator(SyntaxKind.GT, _currentLexeme, startLocation, _currentLocation);
                    }
                case '"':
                    return ReadString(startLocation);
                case '\'':
                    return ReadCharacter(startLocation);

                case '#':
                    {
                        if (Match('#'))
                            return SkipComment(startLocation);

                        return null;
                    }

                default:
                    {
                        if (char.IsLetter(current))
                        {
                            if (MatchLexeme("true") || MatchLexeme("false"))
                                return TokenFactory.BoolLiteral(_currentLexeme, startLocation, _currentLocation);

                            var identifier = ReadIdentifier(startLocation);
                            if (SyntaxFacts.KeywordMap.Contains(_currentLexeme))
                            {
                                var keywordSyntax = SyntaxFacts.KeywordMap.GetValue(_currentLexeme);
                                return TokenFactory.Keyword(keywordSyntax, startLocation, _currentLocation);
                            }

                            return identifier;
                        } else if (char.IsDigit(current))
                            return ReadNumber(startLocation);
                        else if (current == ';')
                            return SkipSemicolons(startLocation);
                        else if (current == '\n')
                            return SkipNewLines(startLocation);
                        else if (char.IsWhiteSpace(current))
                            return SkipWhitespace(startLocation);

                        return null;
                    }
            }
        }

        private Token ReadCharacter(Location location)
        {
            Advance();
            if (_current != '\'')
                Diagnostics.Error("H002", $"Unterminated character", location, _currentLocation);

            Advance();
            return TokenFactory.CharLiteral(_currentLexeme, location, _currentLocation);
        }

        private Token ReadString(Location location)
        {
            while (!_isFinished && _current != '"' && _current != '\n')
                Advance();

            if (_current != '"')
                Diagnostics.Error("H003", $"Unterminated string", location, _currentLocation);

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
                        Diagnostics.Error("H004", "Malformed number", location, _currentLocation);
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
            while (char.IsWhiteSpace((char)_current!)) // fuck you C#
                Advance();

            return TokenFactory.Trivia(TriviaKind.Whitespace, _currentLexeme, location, _currentLocation);
        }

        private Token SkipNewLines(Location location)
        {
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
            return Source.ToCharArray().ElementAtOrDefault(_position + offset);
        }
    }
}
