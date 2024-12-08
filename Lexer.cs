using Heir.Syntax;

namespace Heir
{
    public class Lexer(string source, string fileName = "<anonymous>")
    {
        public string Source { get; } = source;

        private readonly string _fileName = fileName;
        private readonly List<Token> _tokens = [];
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

        public TokenStream GetTokens()
        {
            while (!_isFinished)
            {
                var token = Lex();
                if (token == null) continue;
                _currentLexeme = "";
                _tokens.Add(token);
            }

            _tokens.Add(new Token(SyntaxKind.EOF, "", null, _location));
            return new TokenStream(_tokens.ToArray());
        }

        private Token? Lex()
        {
            var location = _location;
            var current = (char)_current!;
            if ((object)current == null) return null;

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

                default:
                    {
                        if (char.IsLetter(current))
                            return ReadIdentifier();
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

        private Token ReadIdentifier()
        {
            var location = _location;
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

        private char? Peek(int offset)
        {
            return Source.ToCharArray().ElementAtOrDefault(_position + offset);
        }
    }
}
