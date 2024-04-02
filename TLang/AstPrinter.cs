using System.Text;
using TLang.Core.Parsing;

namespace TLang;

internal class AstPrinter : IExpressionVisitor<string> {
    internal string Print(Expression expression) {
        return expression.AcceptVisitor(this);
    }

    public string Visit(BinaryExpression expression) {
        return Parenthesize(expression.Opt.Lexeme, expression.Left, expression.Right);
    }

    public string Visit(GroupingExpression expression) {
        return Parenthesize("group", expression.Expression);
    }

    public string Visit(LiteralExpression expression) {
        return expression.Value == null ? "nil" : expression.Value.ToString();
    }

    public string Visit(UnaryExpression expression) {
        return Parenthesize(expression.Opt.Lexeme, expression.Right);
    }

    private string Parenthesize(string name, params Expression[] expressions) {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);
        foreach (var expression in expressions) {
            builder.Append(' ');
            builder.Append(expression.AcceptVisitor(this));
        }
        builder.Append(')');

        return builder.ToString();
    }
}
