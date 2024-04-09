using System.Globalization;
using TLang.Core.Error;
using TLang.Core.Parsing;
using TLang.Core.Scanning;

namespace TLang.Core.Interpretation;

internal class Interpreter : IExpressionVisitor<object>, IStatementVisitor {
    private RuntimeEnvironment _environment = new();
    private readonly IReport _report;

    public Interpreter(IReport report) {
        _report = report;
    }

    public void Interpret(List<Statement> statements) {
        try {
            foreach (var statement in statements) {
                Execute(statement);
            }
        }
        catch (RuntimeError err) {
            _report.RuntimeError(err);
        }
    }

    #region Expressions

    public object Visit(BinaryExpression expression) {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);

        switch (expression.Opt.Type) {
            case TokenType.Greater:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left <= (double)right;
            case TokenType.Minus:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left - (double)right;
            case TokenType.Plus:
                return left switch {
                    double ld when right is double rd => ld + rd,
                    string ls when right is double rd => ls + rd.ToString(CultureInfo.InvariantCulture),
                    double ld when right is string rs => ld.ToString(CultureInfo.InvariantCulture) + rs,
                    string ls when right is string rs => ls + rs,
                    _ => throw new RuntimeError(expression.Opt, $"Cannot add {left} to {right}")
                };

                // if (left is string  right is string)
                //     return left.ToString() + right.ToString();

            case TokenType.Slash:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left / (double)right;
            case TokenType.Star:
                CheckNumberOperands(expression.Opt, left, right);
                return (double)left * (double)right;
            case TokenType.BangEqual: return !IsEqual(left, right);
            case TokenType.EqualEqual: return IsEqual(left, right);
        }

        throw new Exception("Unknown binary operation.");
    }

    public object Visit(AssignExpression expression) {
        var value = Evaluate(expression.Value);
        _environment.Assign(expression.Name, value);
        return value;
    }

    public object Visit(GroupingExpression expression) {
        return Evaluate(expression.Expression);
    }

    public object Visit(LiteralExpression expression) {
        return expression.Value;
    }

    public object Visit(UnaryExpression expression) {
        var right = Evaluate(expression.Right);

        switch (expression.Opt.Type) {
            case TokenType.Minus: {
                CheckNumberOperands(expression.Opt, right);
                return -(double)right;
            }
            case TokenType.Bang:
                return !IsTruthy(right);
            default:
                throw new Exception("Unknown unary operation.");
        }
    }

    public object? Visit(VariableExpression expression) {
        return _environment.Get(expression.Name);
    }

    #endregion

    #region Statements

    public void Visit(PrintStatement statement) {
        var value = Evaluate(statement.Expression);
        Console.WriteLine(Stringify(value));
    }

    public void Visit(BlockStatement statement) {
        ExecuteBlock(statement.Statements, new RuntimeEnvironment(_environment));
    }

    private void ExecuteBlock(List<Statement> statements, RuntimeEnvironment environment) {
        var previous = _environment;
        try
        {
            _environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public void Visit(ExpressionStatement statement) {
        Evaluate(statement.Expression);
    }

    public void Visit(VarStatement statement) {
        var value = statement.Initializer != null
            ? Evaluate(statement.Initializer)
            : null;

        _environment.Define(statement.Name.Lexeme, value);
    }

    private void Execute(Statement statement) {
        statement.AcceptVisitor(this);
    }

    #endregion

    public static string Stringify(object value) {
        switch (value) {
            case null:
                return "nil";
            case double dv: {
                var text = dv.ToString(CultureInfo.InvariantCulture);
                if (text.EndsWith(".0"))
                    text = text[..^2];
                return text;
            }
            default:
                return value.ToString()!;
        }
    }

    private static void CheckNumberOperands(Token opt, params object[] opds) {
        if (opds.All(o => o is double)) return;
        throw new RuntimeError(opt, "Operand must be a number.");
    }

    private static bool IsTruthy(object value) {
        return value switch {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private static bool IsEqual(object a, object b) {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private object Evaluate(Expression expression) {
        return expression.AcceptVisitor(this);
    }
}
