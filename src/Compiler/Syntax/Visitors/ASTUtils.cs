using Lotus.Syntax.Visitors;

namespace Lotus.Syntax;

using System.Text.Json.Nodes;

internal static class ASTUtils
{
    [Obsolete("NameChecker is deprecated. Please use 'is NameNode' pattern matching instead")]
    internal static readonly NameChecker NameChecker = new();

    internal static readonly TopLevelPrinter TopLevelPrinter = new();

    internal static readonly StatementPrinter StatementPrinter = new();

    internal static readonly ValuePrinter ValuePrinter = new();

    internal static readonly TokenPrinter TokenPrinter = new();

    internal static readonly ConstantChecker ConstantChecker = new();

    internal static readonly TopLevelSerializer TopLevelSerializer = new();

    internal static readonly StatementSerializer StatementSerializer = new();

    internal static readonly ValueSerializer ValueSerializer = new();

    internal static readonly TokenSerializer TokenSerializer = new();

    [Obsolete("ASTHelper.IsName is deprecated. Please use 'is NameNode' pattern matching instead")]
    public static bool IsName(ValueNode node) => NameChecker.IsName(node);

    public static string PrintNode(Node node)
        => node switch {
            ValueNode value => PrintValue(value),
            StatementNode statement => PrintStatement(statement),
            TopLevelNode tl => PrintTopLevel(tl),
            _ => throw new NotImplementedException(
                                           "There's no ToGraphNode() method for type " + node.GetType() + " or any of its base types"
                                       )
        };

    public static string PrintTuple<T>(Tuple<T> tuple, string sep, Func<T, string> transform)
        => PrintToken(tuple.OpeningToken) + MiscUtils.Join(sep, transform, tuple.Items) + PrintToken(tuple.ClosingToken);

    public static string PrintTopLevel(TopLevelNode node) => TopLevelPrinter.Print(node);
    public static string PrintStatement(StatementNode node) => StatementPrinter.Print(node);
    public static string PrintValue(ValueNode node) => ValuePrinter.Print(node);
    public static string PrintTypeName(TypeDecName typeDec) => TopLevelPrinter.Print(typeDec);
    public static string PrintToken(Token token) => TokenPrinter.Print(token);
    public static string PrintUnion<T, U>(Union<T, U> u) where T : Node
                                                         where U : Node
        => u.Match(PrintNode, PrintNode);

    public static JsonObject SerializeNode(Node node)
        => node switch {
            ValueNode value => SerializeValue(value),
            TopLevelNode tl => SerializeTopLevel(tl),
            _ => throw new NotImplementedException("oof" + node.GetType()),
        };

    public static JsonArray SerializeTuple<T>(Tuple<T> tuple, Func<T, JsonObject> transform) => SerializeTuple(tuple.Items, transform);
    public static JsonArray SerializeTuple<T>(ImmutableArray<T> tuple, Func<T, JsonObject> transform) {
        JsonObject[] ret = new JsonObject[tuple.Length];
        for (int i = 0; i < tuple.Length; i++) {
            ret[i] = transform(tuple[i]);
        }
        return new JsonArray(ret);
    }

    public static JsonObject SerializeStructField(StructField field)
        => new JsonObject(){
            {"name", SerializeValue(field.Name)},
            {"type", SerializeValue(field.Type)},
            {"default", SerializeValue(field.DefaultValue)},
        };

    public static JsonObject SerializeFunctionParameter(FunctionParameter field)
        => new JsonObject(){
            {"name", SerializeValue(field.Name)},
            {"type", SerializeValue(field.Type)},
            {"default", SerializeValue(field.DefaultValue)},
        };

    public static JsonObject SerializeTopLevel(TopLevelNode node) => TopLevelSerializer.Serialize(node);
    public static JsonObject SerializeStatement(StatementNode node) => StatementSerializer.Serialize(node);
    public static JsonObject SerializeValue(ValueNode node) => ValueSerializer.Serialize(node);
    public static JsonObject SerializeToken(Token token) => TokenSerializer.Serialize(token);
    public static JsonObject SerializeUnion<T, U>(Union<T, U> u) where T : Node
                                                                 where U : Node
        => u.Match(SerializeNode, SerializeNode);

    public static bool IsContant(ValueNode node) => ConstantChecker.IsContant(node);
}