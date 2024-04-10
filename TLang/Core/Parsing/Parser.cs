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

    internal List<Statement> Parse() {
        var statements = new List<Statement>();
        while (!IsAtEnd()) {
            statements.Add(ParseDeclaration());
        }

        return statements;
    }

    #region Expressions

    private Expression ParseExpression() {
        return ParseAssignment();
    }

    private Expression ParseAssignment() {
        var expr = ParseOr();

        if (Match(TokenType.Equal)) {
            var equals = Previous();
            var value = ParseAssignment();

            if (expr is VariableExpression varExp) {
                return new AssignExpression(varExp.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expression ParseOr() {
        var expr = ParseAnd();

        while (Match(TokenType.Or)) {
            var opt = Previous();
            var right = ParseAnd();
            expr = new LogicalExpression(expr, opt, right);
        }

        return expr;
    }

    private Expression ParseAnd() {
        var expr = ParseEquality();

        while (Match(TokenType.And)) {
            var opt = Previous();
            var right = ParseEquality();
            expr = new LogicalExpression(expr, opt, right);
        }

        return expr;
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

        if (Match(TokenType.Identifier)) {
            return new VariableExpression(Previous());
        }

        if (Match(TokenType.LeftParen)) {
            var expr = ParseExpression();
            ConsumeOrThrow(TokenType.RightParen, "Expect ')' after expression.");
            return new GroupingExpression(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    #endregion

    #region Statements

    private Statement ParseStatement() {
        if (Match(TokenType.For)) return ParseForStatement();
        if (Match(TokenType.If)) return ParseIfStatement();
        if (Match(TokenType.Print)) return ParsePrintStatement();
        if (Match(TokenType.While)) return ParseWhileStatement();
        if (Match(TokenType.LeftBrace)) return new BlockStatement(ParseBlockStatement());

        return ParseExpressionStatement();
    }

    private Statement ParseForStatement() {
        ConsumeOrThrow(TokenType.LeftParen, "Expect '(' after 'for'.");

        Statement? initializer;
        if (Match(TokenType.Semicolon)) {
            initializer = null;
        } else if (Match(TokenType.Var)) {
            initializer = ParseVarDeclaration();
        } else {
            initializer = ParseExpressionStatement();
        }

        Expression? condition = null;
        if (!Check(TokenType.Semicolon)) {
            condition = ParseExpression();
        }
        ConsumeOrThrow(TokenType.Semicolon, "Expect ';' after loop condition.");

        Expression? increment = null;
        if (!Check(TokenType.RightParen)) {
            increment = ParseExpression();
        }
        ConsumeOrThrow(TokenType.RightParen, "Expect ')' after for clauses.");

        var body = ParseStatement();

        if (increment != null) {
            body = new BlockStatement([body, new ExpressionStatement(increment)]);
        }

        condition ??= new LiteralExpression(true);
        body = new WhileStatement(condition, body);

        if (initializer != null) {
            body = new BlockStatement([initializer, body]);
        }

        return body;
    }

    private Statement ParseWhileStatement() {
        ConsumeOrThrow(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = ParseExpression();
        ConsumeOrThrow(TokenType.RightParen, "Expect ')' after condition.");
        var body = ParseStatement();
        return new WhileStatement(condition, body);
    }

    private List<Statement> ParseBlockStatement() {
        var statements = new List<Statement>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd()) {
            statements.Add(ParseDeclaration());
        }

        ConsumeOrThrow(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }

    private Statement ParseIfStatement() {
        ConsumeOrThrow(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = ParseExpression();
        ConsumeOrThrow(TokenType.RightParen, "Expect ')' after if condition.");

        var thenBranch = ParseStatement();
        var elseBranch = Match(TokenType.Else) ? ParseStatement() : null;

        return new IfStatement(condition, thenBranch, elseBranch);
    }

    private Statement ParsePrintStatement() {
        var value = ParseExpression();
        ConsumeOrThrow(TokenType.Semicolon, "Expect ';' after value.");
        return new PrintStatement(value);
    }

    private Statement ParseExpressionStatement() {
        var expression = ParseExpression();
        ConsumeOrThrow(TokenType.Semicolon, "Expect ';' after value.");
        return new ExpressionStatement(expression);
    }

    private Statement ParseDeclaration() {
        try {
            return Match(TokenType.Var) ? ParseVarDeclaration() : ParseStatement();
        } catch (ParseException) {
            Synchronize();
            return null;
        }
    }

    private Statement ParseVarDeclaration() {
        var name = ConsumeOrThrow(TokenType.Identifier, "Expect variable name.");
        Expression initializer = null;
        if (Match(TokenType.Equal)) {
            initializer = ParseExpression();
        }

        ConsumeOrThrow(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarStatement(name, initializer);
    }

    #endregion

    private Token ConsumeOrThrow(TokenType type, string message) {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private ParseException Error(Token token, string message) {
        ReportError(token, message);
        return new ParseException();
    }

    private  void ReportError(Token token, string message) {
        if (token.Type == TokenType.Eof) {
            _report.Write(token.Line, "at end", message);
        } else {
            _report.Write(token.Line, "at '" + token.Lexeme + "'", message);
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
