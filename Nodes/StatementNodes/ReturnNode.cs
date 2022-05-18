using System.Diagnostics.CodeAnalysis;

public record ReturnNode(ValueNode Value, Token Token, bool IsValid = true)
: StatementNode(
    Token,
    Value != ValueNode.NULL
        ? new LocationRange(Token.Location, Value.Location)
        : Token.Location,
    IsValid
)
{
    public new static readonly ReturnNode NULL = new(ValueNode.NULL, Token.NULL, false);

    public bool IsReturningValue => Value != ValueNode.NULL;

    [System.Diagnostics.DebuggerHidden()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.Diagnostics.DebuggerNonUserCode()]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}
