namespace TLang.Core.Parsing;

internal abstract record Expression {
    internal abstract T AcceptVisitor<T>(IExpressionVisitor<T> visitor);
}
