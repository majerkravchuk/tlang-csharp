namespace TLang.Core.Scanning;

public class Scanner {
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private readonly IReport _report;

    private int _start;
    private int _current;
    private int _line = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new() {
        { "and",    TokenType.And },
        { "class",  TokenType.Class },
        { "else",   TokenType.Else },
        { "false",  TokenType.False },
        { "for",    TokenType.For },
        { "fun",    TokenType.Fun },
        { "if",     TokenType.If },
        { "nil",    TokenType.Nil },
        { "or",     TokenType.Or },
        { "print",  TokenType.Print },
        { "return", TokenType.Return },
        { "super",  TokenType.Super },
        { "this",   TokenType.This },
        { "true",   TokenType.True },
        { "var",    TokenType.Var },
        { "while",  TokenType.While }
    };

    public Scanner(string source, IReport report) {
        _source = source;
        _report = report;
    }

    public List<Token> ScanTokens() {
        while (!IsAtEnd()) {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    private void ScanToken() {
        var c = Advance();
        switch (c) {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (Match('/')) {
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else {
                    AddToken(TokenType.Slash);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                _line++;
                break;
            case '"': ScanString(); break;
            default:
                if (IsDigit(c)) {
                    ScanNumber();
                }
                else if (IsAlpha(c)) {
                    ScanIdentifier();
                }
                else {
                    _report.Error(_line, "Unexpected character.");
                }

                break;
        }
    }

    private void ScanIdentifier() {
        while (IsAlphaNumeric(Peek()))
            Advance();

        var text = _source.Substring(_start, _current - _start);
        var type = Keywords.GetValueOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }

    private bool IsAlphaNumeric(char c) {
        return IsAlpha(c) || IsDigit(c);
    }

    private bool IsAlpha(char c) {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private void ScanString() {
        while (Peek() != '"' && !IsAtEnd()) {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd()) {
            _report.Error(_line, "Unterminated string.");
            return;
        }

        // The closing ".
        Advance();

        var value = _source.Substring(_start + 1, _current - _start - 2);
        AddToken(TokenType.String, value);
    }

    private char PeekNext() {
        return _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    }

    private void ScanNumber() {
        while (IsDigit(Peek()))
            Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext())) {
            // Consume the "."
            Advance();

            while (IsDigit(Peek()))
                Advance();
        }

        AddToken(TokenType.Number, double.Parse(_source.Substring(_start, _current - _start)));
    }

    private void AddToken(TokenType type, object? literal = null) {
        var text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private bool IsAtEnd() {
        return _current >= _source.Length;
    }

    private char Advance() {
        return _source[_current++];
    }

    private char Peek() {
        return IsAtEnd() ? '\0' : _source[_current];
    }

    private bool IsDigit(char c) {
        return c is >= '0' and <= '9';
    }

    private bool Match(char expected) {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }
}
