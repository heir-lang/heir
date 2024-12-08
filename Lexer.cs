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
        private int _position = 0;
        private int _line = 1;
        private int _column = 0;

        public TokenStream GetTokens()
        {
            while (!_isFinished)
            {
                var token = Lex();
                if (token == null) continue;
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
                        var token = TokenFactory.Operator(SyntaxKind.Plus, _current.ToString()!, location);
                        Advance();

                        if (_current == '+')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.PlusPlus, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }
                        else if (_current == '=')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.PlusEqual, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }

                        return token;
                    }
                case '-':
                    {
                        var location = _location;
                        var token = TokenFactory.Operator(SyntaxKind.Minus, _current.ToString()!, location);
                        Advance();

                        if (_current == '-')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.MinusMinus, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }
                        else if (_current == '=')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.MinusEqual, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }

                        return token;
                    }
                case '*':
                    {
                        var location = _location;
                        var token = TokenFactory.Operator(SyntaxKind.Star, _current.ToString()!, location);
                        Advance();

                        if (_current == '=')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.StarEqual, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }

                        return token;
                    }
                case '/':
                    {
                        var location = _location;
                        var token = TokenFactory.Operator(SyntaxKind.Slash, _current.ToString()!, location);
                        Advance();

                        if (_current == '/')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.SlashSlash, _current.ToString()!, location);
                            Advance();

                            if (_current == '=')
                            {
                                var finalToken = TokenFactory.Operator(SyntaxKind.SlashSlashEqual, _current.ToString()!, location);
                                Advance();
                                return finalToken;
                            }

                            return newToken;
                        }
                        else if (_current == '=')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.SlashEqual, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }

                        return token;
                    }
                case '%':
                    {
                        var location = _location;
                        var token = TokenFactory.Operator(SyntaxKind.Percent, _current.ToString()!, location);
                        Advance();

                        if (_current == '=')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.PercentEqual, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }

                        return token;
                    }
                case '^':
                    {
                        var location = _location;
                        var token = TokenFactory.Operator(SyntaxKind.Carat, _current.ToString()!, location);
                        Advance();

                        if (_current == '=')
                        {
                            var newToken = TokenFactory.Operator(SyntaxKind.CaratEqual, _current.ToString()!, location);
                            Advance();
                            return newToken;
                        }

                        return token;
                    }

                default:
                    {
                        if (_current == '\n')
                        {
                            NewLine();
                            return null;
                        }
                        if (string.IsNullOrEmpty(_current.ToString()))
                        {
                            SkipWhitespace();
                            return null;
                        }

                        Advance();
                        return null;
                    }
            }
        }

        private void SkipWhitespace()
        {
            while (string.IsNullOrEmpty(_current.ToString()))
                Advance();
        }

        private void NewLine()
        {
            _line++;
            _column = 0;
        }

        private void Advance()
        {
            _position++;
            _column++;
        }

        private char? Peek(int offset)
        {
            return Source.ToCharArray().ElementAtOrDefault(_position + offset);
        }
    }
}