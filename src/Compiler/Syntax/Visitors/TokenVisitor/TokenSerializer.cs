namespace Lotus.Syntax.Visitors;

using SerializedNode = System.Text.Json.Nodes.JsonObject;

internal sealed class TokenSerializer : ITokenVisitor<SerializedNode>
{
    public SerializedNode Default(Token token)
        => token.Kind == TokenKind.EOF ?
            new SerializedNode(){{"type", "TokenEOF"}} :
            new SerializedNode(){
                {"type", "Token"},
                {"kind", token.Kind.ToString()},
                {"representation", token.Representation},
            };

    public SerializedNode Visit(IdentToken token)
        => new SerializedNode(){
            {"type", "IdentToken"},
            {"representation", token.Representation},
        };

    public SerializedNode Default(TriviaToken? token) => Default(token is null ? Token.NULL : token as Token);

    public SerializedNode Serialize(Token token) => token.Accept(this);
}