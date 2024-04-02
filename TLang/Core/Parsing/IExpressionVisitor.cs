namespace TLang.Core.Parsing;

internal interface IExpressionVisitor<T> {
    T Visit(BinaryExpression expression);
    // T Visit(CallExpression expression);
    // T Visit(GetExpression expression);
    T Visit(GroupingExpression expression);
    T Visit(LiteralExpression expression);
    // T Visit(LogicalExpression expression);
    // T Visit(SetExpression expression);
    // T Visit(SuperExpression expression);
    // T Visit(ThisExpression expression);
    T Visit(UnaryExpression expression);
    // T Visit(VariableExpression expression);
}
