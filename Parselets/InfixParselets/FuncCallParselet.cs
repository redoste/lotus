public sealed class FuncCallParselet : IInfixParselet<FunctionCallNode>
{
    public Precedence Precedence {
        get => Precedence.FuncCall;
    }

    public FunctionCallNode Parse(Parser parser, Token leftParenToken, ValueNode function) {
        if (leftParenToken != "(")
            throw Logger.Fatal(new InvalidCallException(leftParenToken.Location));

        var isValid = true;

        /*if (!Utilities.IsName(function)) {
            Logger.Error(new UnexpectedValueTypeException(
                node: function,
                context: "as a function name",
                expected: "a name (simple (`someFunc`) or a qualified one (`object.toString`)"
            ));

            isValid = false;
        }*/

        // reconsume the '(' for the ConsumeCommaSeparatedList() function
        parser.Tokenizer.Reconsume();

        var args = parser.ConsumeCommaSeparatedValueList("(", ")", ref isValid);

        return new FunctionCallNode(args, function, function.Token, isValid);
    }
}
