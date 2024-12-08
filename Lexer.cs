using Heir.Syntax;

namespace Heir
{
    public class Lexer(string source, string fileName = "<anonymous>")
    {
        public string Source { get; } = source;

        private readonly string _fileName = fileName;
        private List<Token> _tokens = [];
        private string _currentLexeme = "";
        private int _position = 0;
        private int _line = 1;
        private int _column = 0;
        private Location _location
        {
            get => new Location(_fileName, _line, _column);
        }
        private bool _isFinished
        {
            get => _position >= Source.Length;
        }
        private char? _current
        {
            get => Peek(0);
        }

        public TokenStream GetTokens(bool noTrivia = false)
        {
            while (!_isFinished)
            {
                var token = Lex();
                if (token == null)
                {
                    // unexpected character (_current)
                    continue;
                }

                _currentLexeme = "";
                _tokens.Add(token);
            }

            _tokens.Add(TokenFactory.Trivia("", _location, TriviaKind.EOF));
            return new TokenStream(_tokens.Where(token => noTrivia ? !token.IsKind(SyntaxKind.Trivia) : true).ToArray());
        }

        private Token? Lex()
        {
            var location = _location;
            var current = (char)_current!;
            if (_current == null) return null;

            Advance();
            switch (current)
            {
                case '+':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Plus, _currentLexeme, location);
                        if (Match('+'))
                            return TokenFactory.Operator(SyntaxKind.PlusPlus, _currentLexeme, location);
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PlusEquals, _currentLexeme, location);

                        return token;
                    }
                case '-':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Minus, _currentLexeme, location);
                        if (Match('-'))
                            return TokenFactory.Operator(SyntaxKind.MinusMinus, _currentLexeme, location);
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.MinusEquals, _currentLexeme, location);

                        return token;
                    }
                case '*':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Star, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.StarEquals, _currentLexeme, location);

                        return token;
                    }
                case '/':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Slash, _currentLexeme, location);
                        if (Match('/'))
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.SlashSlash, _currentLexeme, location);
                            if (Match('='))
                                return TokenFactory.Operator(SyntaxKind.SlashSlashEquals, _currentLexeme, location);

                            return newToken;
                        }
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.SlashEquals, _currentLexeme, location);

                        return token;
                    }
                case '%':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Percent, _current.ToString()!, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PercentEquals, _currentLexeme, location);

                        return token;
                    }
                case '^':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Carat, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.CaratEquals, _currentLexeme, location);

                        return token;
                    }

                case '=':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Equals, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.EqualsEquals, _currentLexeme, location);

                        return token;
                    }
                case '!':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Bang, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.BangEquals, _currentLexeme, location);

                        return token;
                    }
                case '?':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Question, _currentLexeme, location);
                        if (Match('?'))
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.QuestionQuestion, _currentLexeme, location);
                            if (Match('='))
                                return TokenFactory.Operator(SyntaxKind.QuestionQuestionEquals, _currentLexeme, location);

                            return newToken;
                        }

                        return token;
                    }
                case ':':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.Colon, _currentLexeme, location);
                        if (Match(':'))
                            return TokenFactory.Operator(SyntaxKind.ColonColon, _currentLexeme, location);

                        return token;
                    }
                case '.':
                    return TokenFactory.Operator(SyntaxKind.Dot, _currentLexeme, location);
                case ',':
                    return TokenFactory.Operator(SyntaxKind.Comma, _currentLexeme, location);

                case '(':
                    return TokenFactory.Operator(SyntaxKind.LParen, _currentLexeme, location);
                case ')':
                    return TokenFactory.Operator(SyntaxKind.RParen, _currentLexeme, location);
                case '[':
                    return TokenFactory.Operator(SyntaxKind.LBracket, _currentLexeme, location);
                case ']':
                    return TokenFactory.Operator(SyntaxKind.RBracket, _currentLexeme, location);
                case '{':
                    return TokenFactory.Operator(SyntaxKind.LBrace, _currentLexeme, location);
                case '}':
                    return TokenFactory.Operator(SyntaxKind.RBrace, _currentLexeme, location);
                case '<':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.LT, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.LTE, _currentLexeme, location);

                        return token;
                    }
                case '>':
                    {
                        var token = TokenFactory.Operator(SyntaxKind.GT, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.GTE, _currentLexeme, location);

                        return token;
                    }
                case '"':
                    return ReadString(location);
                case '\'':
                    return ReadCharacter(location);

                case '#':
                    {
                        if (Match('#'))
                            return SkipComment();

                        return null;
                    }

                default:
                    {
                        if (char.IsLetter(current))
                        {
                            if (MatchLexeme("none"))
                                return TokenFactory.NoneLiteral(location);
                            else if (MatchLexeme("true") || MatchLexeme("false"))
                                return TokenFactory.BoolLiteral(_currentLexeme, location);
                            else if (SyntaxFacts.KeywordMap.Contains(_currentLexeme))
                            {
                                var keywordSyntax = SyntaxFacts.KeywordMap.GetValue(_currentLexeme);
                                return TokenFactory.Keyword(keywordSyntax, location);
                            }

                            return ReadIdentifier(location);
                        } else if (char.IsDigit(current))
                            return ReadNumber(location);
                        else if (current == ';')
                            return SkipSemicolons();
                        else if (current == '\n')
                            return SkipNewLines();
                        else if (char.IsWhiteSpace(current))
                            return SkipWhitespace();

                        Advance();
                        return null;
                    }
            }
        }

        private Token ReadCharacter(Location location)
        {
            Advance();
            if (_current != '\'')
            {
                // unexpected character, expected '
            }
            Advance();

            return TokenFactory.CharLiteral(_currentLexeme, location);
        }

        private Token ReadString(Location location)
        {
            while (!_isFinished && _current != '"')
                Advance();

            Advance();
            return TokenFactory.StringLiteral(_currentLexeme, location);
        }

        private Token ReadNumber(Location location)
        {
            if (Peek(-1) == '0')
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
                    decimalUsed = true;

                Advance();
            }

            if (decimalUsed)
                return TokenFactory.FloatLiteral(_currentLexeme, location);
            else
                return TokenFactory.IntLiteral(_currentLexeme, location);
        }

        private Token ReadNonDecimalNumber(Location location, int radix)
        {
            Advance();
            while (char.IsLetterOrDigit((char)_current!)) // fuck you C#
                Advance();

            return TokenFactory.IntLiteral(_currentLexeme, location, radix);
        }

        private Token ReadIdentifier(Location location)
        {
            while (char.IsLetterOrDigit((char)_current!)) // fuck you C#
                Advance();

            return TokenFactory.Identifier(_currentLexeme, location);
        }

        private Token SkipWhitespace()
        {
            var location = _location;
            while (char.IsWhiteSpace((char)_current!)) // fuck you C#
                Advance();

            return TokenFactory.Trivia(_currentLexeme, location, TriviaKind.Whitespace);
        }

        private Token SkipNewLines()
        {
            var location = _location;
            while (_current == '\n')
            {
                _position++; // Advance() but w/o adding to _column for performance reasons
                _line++;
            }

            _column = 0;
            return TokenFactory.Trivia(_currentLexeme, location, TriviaKind.Newlines);
        }

        private Token SkipSemicolons()
        {
            var location = _location;
            while (_current == ';')
                Advance();

            return TokenFactory.Trivia(_currentLexeme, location, TriviaKind.Semicolons);
        }

        private Token SkipComment()
        {
            var location = _location;
            while (!_isFinished && _current != '\n')
                Advance();

            return TokenFactory.Trivia(_currentLexeme, location, TriviaKind.Comment);
        }

        private bool MatchLexeme(string lexeme)
        {
            var characters = lexeme.ToCharArray().ToList();
            var isMatch = Peek(-1) == characters.First();
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
