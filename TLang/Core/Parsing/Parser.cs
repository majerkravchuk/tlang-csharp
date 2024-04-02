using TLang.Core.Scanning;

namespace TLang.Core.Parsing;

internal class Parser {
    private readonly IReport _report;
    private readonly List<Token> _tokens;
    private int _current;

    private class ParseException : ApplicationException;

    internal Parser(List<Token> tokens, IReport report) {
        _tokens = tokens;
        _report = report;
    }

    internal Expression Parse() {
        try {
            return ParseExpression();
        } catch (ParseException) {
            return null;
        }
    }

    private Expression ParseExpression() {
        return ParseEquality();
    }

    private Expression ParseEquality() {
        var expr = ParseComparison();
        while (Match(TokenType.BangEqual) || Match(TokenType.EqualEqual)) {
            expr = new BinaryExpression(expr, Previous(), ParseComparison());
        }

        return expr;
    }

    private Expression ParseComparison() {
        var expr = ParseTerm();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual)) {
            expr = new BinaryExpression(expr, Previous(), ParseTerm());
        }

        return expr;
    }

    private Expression ParseTerm() {
        var expr = ParseFactor();

        while (Match(TokenType.Minus, TokenType.Plus)) {
            expr = new BinaryExpression(expr, Previous(), ParseFactor());
        }

        return expr;
    }

    private Expression ParseFactor() {
        var expr = ParseUnary();

        while (Match(TokenType.Slash, TokenType.Star)) {
            expr = new BinaryExpression(expr, Previous(), ParseUnary());
        }

        return expr;
    }

    private Expression ParseUnary() {
        return Match(TokenType.Bang, TokenType.Minus)
            ? new UnaryExpression(Previous(), ParseUnary())
            : ParsePrimary();
    }

    private Expression ParsePrimary() {
        if (Match(TokenType.False)) return new LiteralExpression(false);
        if (Match(TokenType.True)) return new LiteralExpression(true);
        if (Match(TokenType.Nil)) return new LiteralExpression(null);

        if (Match(TokenType.Number, TokenType.String)) {
            return new LiteralExpression(Previous().Literal);
        }

        if (Match(TokenType.LeftParen)) {
            var expr = ParseExpression();
            ConsumeOrThrow(TokenType.RightParen, "Expect ')' after expression.");
            return new GroupingExpression(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private void ConsumeOrThrow(TokenType type, string message) {
        if (Check(type)) Advance();

        throw Error(Peek(), message);
    }

    private ParseException Error(Token token, string message) {
        ReportError(token, message);
        return new ParseException();
    }

    private  void ReportError(Token token, string message) {
        if (token.Type == TokenType.Eof) {
            _report.Write(token.Line, " at end", message);
        } else {
            _report.Write(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    private void Synchronize() {
        Advance();

        while (!IsAtEnd()) {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type) {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }

    private bool Match(params TokenType[] tokens) {
        if (!tokens.Any(Check)) return false;
        Advance();
        return true;

    }

    private bool IsAtEnd() {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek() {
        return _tokens[_current];
    }

    private Token Previous() {
        return _tokens[_current - 1];
    }

    private bool Check(TokenType token) {
        if (IsAtEnd()) return false;
        return Peek().Type == token;
    }

    private Token Advance() {
        if (!IsAtEnd()) _current++;
        return Previous();
    }
}
