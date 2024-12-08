using Heir.Syntax;

namespace Heir
{
    public class Lexer(string source, string fileName = "<anonymous>")
    {
        public string Source { get; } = source;

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

        private readonly string _fileName = fileName;
        private readonly List<Token> _tokens = [];
        private string _currentLexeme = "";
        private int _position = 0;
        private int _line = 1;
        private int _column = 0;

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
            switch (_current)
            {
                case '+':
                    {
                        var location = _location;
                        Advance();

                        var token = TokenFactory.Operator(SyntaxKind.Plus, _currentLexeme, location);
                        if (Match('+'))
                            return TokenFactory.Operator(SyntaxKind.PlusPlus, _currentLexeme, location);
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PlusEquals, _currentLexeme, location);

                        return token;
                    }
                case '-':
                    {
                        var location = _location;
                        Advance();

                        var token = TokenFactory.Operator(SyntaxKind.Minus, _currentLexeme, location);
                        if (Match('-'))
                            return TokenFactory.Operator(SyntaxKind.MinusMinus, _currentLexeme, location);
                        else if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.MinusEquals, _currentLexeme, location);

                        return token;
                    }
                case '*':
                    {
                        var location = _location;
                        Advance();

                        var token = TokenFactory.Operator(SyntaxKind.Star, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.StarEquals, _currentLexeme, location);

                        return token;
                    }
                case '/':
                    {
                        var location = _location;
                        Advance();

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
                        var location = _location;
                        Advance();

                        var token = TokenFactory.Operator(SyntaxKind.Percent, _current.ToString()!, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.PercentEquals, _currentLexeme, location);

                        return token;
                    }
                case '^':
                    {
                        var location = _location;
                        Advance();

                        var token = TokenFactory.Operator(SyntaxKind.Carat, _currentLexeme, location);
                        if (Match('='))
                            return TokenFactory.Operator(SyntaxKind.CaratEquals, _currentLexeme, location);

                        return token;
                    }

                default:
                    {
                        var current = (char)_current!; // kill me
                        if (char.IsLetter(current))
                        {
                            return ReadIdentifier();
                        }
                        else if (current == '\n')
                        {
                            SkipNewLines();
                            return null;
                        }
                        else if (char.IsWhiteSpace(current))
                        {
                            SkipWhitespace();
                            return null;
                        }

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

            var lexeme = _currentLexeme;
            _currentLexeme = "";
            return TokenFactory.Trivia(lexeme, location, TriviaKind.Whitespace);
        }

        private Token SkipNewLines()
        {
            var location = _location;
            while (_current == '\n')
            {
                _position++; // Advance() but w/o adding to _column for performance reasons
                _line++;
            }

            var lexeme = _currentLexeme;
            _currentLexeme = "";
            _column = 0;
            return TokenFactory.Trivia(lexeme, location, TriviaKind.Newlines);
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