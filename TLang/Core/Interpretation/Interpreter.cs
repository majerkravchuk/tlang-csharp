using System.Globalization;
using TLang.Core.Error;
using TLang.Core.Parsing;
using TLang.Core.Scanning;

namespace TLang.Core.Interpretation;

internal class Interpreter : IExpressionVisitor<object> {
    public object Interpret(Expression expression) {
        return Evaluate(expression);
    }

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
