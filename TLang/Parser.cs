namespace TLang;

public class Parser {
    private List<Token> Tokens { get; set; }
    private int _current = 0;

    private class ParseException : ApplicationException;

    public Parser(List<Token> tokens) {
        Tokens = tokens;
    }

    public Expression Parse() {
        try {
            return ParseExpression();
        } catch (ParseException err) {
            return null;
        }
    }

    private Expression ParseExpression() {
        return ParseEquality();
    }

    private Expression ParseEquality() {
        var expr = ParseComparison();
        while (Match(TokenType.BangEqual) || Match(TokenType.EqualEqual)) {
            expr = new Expression.BinaryExpression(expr, Previous(), ParseComparison());
        }

        return expr;
    }

    private Expression ParseComparison() {
        var expr = ParseTerm();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual)) {
            expr = new Expression.BinaryExpression(expr, Previous(), ParseTerm());
        }

        return expr;
    }

    private Expression ParseTerm() {
        var expr = ParseFactor();

        while (Match(TokenType.Minus, TokenType.Plus)) {
            expr = new Expression.BinaryExpression(expr, Previous(), ParseFactor());
        }

        return expr;
    }

    private Expression ParseFactor() {
        var expr = ParseUnary();

        while (Match(TokenType.Slash, TokenType.Star)) {
            expr = new Expression.BinaryExpression(expr, Previous(), ParseUnary());
        }

        return expr;
    }

    private Expression ParseUnary() {
        return Match(TokenType.Bang, TokenType.Minus)
            ? new Expression.UnaryExpression(Previous(), ParseUnary())
            : ParsePrimary();
    }

    private Expression ParsePrimary() {
        if (Match(TokenType.False)) return new Expression.LiteralExpression(false);
        if (Match(TokenType.True)) return new Expression.LiteralExpression(true);
        if (Match(TokenType.Nil)) return new Expression.LiteralExpression(null);

        if (Match(TokenType.Number, TokenType.String)) {
            return new Expression.LiteralExpression(Previous().Literal);
        }

        if (Match(TokenType.LeftParen)) {
            var expr = ParseExpression();
            ConsumeOrThrow(TokenType.RightParen, "Expect ')' after expression.");
            return new Expression.GroupingExpression(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Token ConsumeOrThrow(TokenType type, string message) {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private static ParseException Error(Token token, string message) {
        ReportError(token, message);
        return new ParseException();
    }

    private static void ReportError(Token token, string message) {
        if (token.Type == TokenType.Eof) {
            Report.Write(token.Line, " at end", message);
        } else {
            Report.Write(token.Line, " at '" + token.Lexeme + "'", message);
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
        return Tokens[_current];
    }

    private Token Previous() {
        return Tokens[_current - 1];
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
