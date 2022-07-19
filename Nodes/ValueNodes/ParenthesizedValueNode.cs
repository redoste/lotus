public record ParenthesizedValueNode : TupleNode
{
    public new static readonly ParenthesizedValueNode NULL = new(ValueNode.NULL, Token.NULL, Token.NULL, false);

    public ValueNode Value => Count == 0 ? ValueNode.NULL : base.Items[0];

    public ParenthesizedValueNode(ValueNode value, Token leftParen, Token rightParen, bool isValid = true)
        : base(new[] {value}, leftParen, rightParen, isValid) { }

    [DebuggerHidden()]
    [DebuggerStepThrough()]
    [DebuggerNonUserCode()]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override T Accept<T>(IValueVisitor<T> visitor) => visitor.Visit(this);
}