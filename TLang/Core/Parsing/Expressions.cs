using TLang.Core.Scanning;

namespace TLang.Core.Parsing;

internal record BinaryExpression(Expression Left, Token Opt, Expression Right) : Expression
{
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal record GroupingExpression(Expression Expression) : Expression {
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal record UnaryExpression(Token Opt, Expression Right) : Expression {
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal record LiteralExpression(object Value) : Expression {
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}
