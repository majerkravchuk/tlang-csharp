using TLang.Core.Scanning;

namespace TLang.Core.Parsing;

internal abstract record Expression {
    internal abstract T AcceptVisitor<T>(IExpressionVisitor<T> visitor);
}

internal interface IExpressionVisitor<T> {
    T Visit(BinaryExpression expression);
    T Visit(AssignExpression expression);
    // T Visit(CallExpression expression);
    // T Visit(GetExpression expression);
    T Visit(GroupingExpression expression);
    T Visit(LiteralExpression expression);
    T Visit(LogicalExpression expression);
    // T Visit(SetExpression expression);
    // T Visit(SuperExpression expression);
    // T Visit(ThisExpression expression);
    T Visit(UnaryExpression expression);
    T? Visit(VariableExpression expression);
}

internal record LogicalExpression(Expression Left, Token Opt, Expression Right) : Expression
{
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal record BinaryExpression(Expression Left, Token Opt, Expression Right) : Expression
{
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal record AssignExpression(Token Name, Expression Value) : Expression
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

internal record VariableExpression (Token Name) : Expression {
    internal override T AcceptVisitor<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}
