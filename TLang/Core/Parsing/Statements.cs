using TLang.Core.Scanning;

namespace TLang.Core.Parsing;

internal abstract record Statement {
    internal abstract void AcceptVisitor(IStatementVisitor visitor);
}

internal interface IStatementVisitor {
    void Visit(PrintStatement statement);
    void Visit(BlockStatement statement);
    void Visit(ExpressionStatement statement);
    void Visit(VarStatement statement);
}

internal record PrintStatement(Expression Expression) : Statement
{
    internal override void AcceptVisitor(IStatementVisitor visitor) => visitor.Visit(this);
}

internal record BlockStatement(List<Statement> Statements) : Statement
{
    internal override void AcceptVisitor(IStatementVisitor visitor) => visitor.Visit(this);
}

internal record ExpressionStatement(Expression Expression) : Statement
{
    internal override void AcceptVisitor(IStatementVisitor visitor) => visitor.Visit(this);
}

internal record VarStatement(Token Name, Expression? Initializer) : Statement
{
    internal override void AcceptVisitor(IStatementVisitor visitor) => visitor.Visit(this);
}
