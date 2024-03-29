namespace TLang;

public abstract class Expression {
    public abstract T Accept<T>(IExpressionVisitor<T> visitor);

    public interface IExpressionVisitor<T>
    {
        // T VisitAssignExpression(AssignExpression expression);
        T VisitBinaryExpression(BinaryExpression expression);
        // T VisitCallExpression(CallExpression expression);
        // T VisitGetExpression(GetExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        // T VisitLogicalExpression(LogicalExpression expression);
        // T VisitSetExpression(SetExpression expression);
        // T VisitSuperExpression(SuperExpression expression);
        // T VisitThisExpression(ThisExpression expression);
        T VisitUnaryExpression(UnaryExpression expression);
        // T VisitVariableExpression(VariableExpression expression);
    }

    public class BinaryExpression(Expression left, Token opt, Expression right) : Expression {
        public Expression Left { get; } = left;
        public Token Opt { get; } = opt;
        public Expression Right { get; } = right;

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }

    public class GroupingExpression(Expression expression) : Expression {
        public Expression Expression { get; } = expression;

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }

    public class UnaryExpression(Token opt, Expression right) : Expression {
        public Token Opt { get; } = opt;
        public Expression Right { get; } = right;

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }

    public class LiteralExpression(object? value) : Expression {
        public object? Value { get; } = value;

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }
}
