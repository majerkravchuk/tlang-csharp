using System.Text;

namespace TLang;

public class AstPrinter : Expression.IExpressionVisitor<string> {
    public string Print(Expression expression) {
        return expression.Accept(this);
    }
    public string VisitBinaryExpression(Expression.BinaryExpression expression) {
        return Parenthesize(expression.Opt.Lexeme, expression.Left, expression.Right);
    }

    public string VisitGroupingExpression(Expression.GroupingExpression expression) {
        return Parenthesize("group", expression.Expression);
    }

    public string VisitLiteralExpression(Expression.LiteralExpression expression) {
        return expression.Value == null ? "nil" : expression.Value.ToString();
    }

    public string VisitUnaryExpression(Expression.UnaryExpression expression) {
        return Parenthesize(expression.Opt.Lexeme, expression.Right);
    }

    private string Parenthesize(string name, params Expression[] expressions) {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);
        foreach (var expression in expressions) {
            builder.Append(' ');
            builder.Append(expression.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }
}

class AstPrinterImpl : AstPrinter { }
