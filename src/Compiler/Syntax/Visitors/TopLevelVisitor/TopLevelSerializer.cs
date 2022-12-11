namespace Lotus.Syntax.Visitors;

using SerializedNode = System.Text.Json.Nodes.JsonObject;

internal sealed class TopLevelSerializer : ITopLevelVisitor<SerializedNode>
{
    public SerializedNode Default(TopLevelNode node)
        => new SerializedNode(){
            {"type", "TopLevelNode"},
            {"token", ASTUtils.SerializeToken(node.Token)}
        };

    public SerializedNode Visit(FromNode node)
        => new SerializedNode(){
            {"type", "FromNode"},
            {"origin", ASTUtils.SerializeUnion(node.OriginName)},
        };

    public SerializedNode Visit(TopLevelStatementNode node)
        => new SerializedNode(){
            {"type", "TopLevelStatementNode"},
            {"statement", ASTUtils.SerializeStatement(node.Statement)}
        };

    public SerializedNode Visit(EnumNode node)
        => new SerializedNode(){
            {"type", "EnumNode"},
            {"access", node.AccessLevel.ToString()},
            {"name", Serialize(node.Name)},
            {"values", ASTUtils.SerializeTuple(node.Values, ASTUtils.SerializeValue)},
        };

    public SerializedNode Visit(ImportNode node)
        => new SerializedNode(){
            {"type", "ImportNode"},
            {"from", Visit(node.FromStatement)},
            {"names", ASTUtils.SerializeTuple(node.Names, ASTUtils.SerializeValue)},
        };

    public SerializedNode Visit(NamespaceNode node)
        => new SerializedNode(){
            {"type", "NamespaceNode"},
            {"access", node.AccessLevel.ToString()},
            {"name", ASTUtils.SerializeValue(node.Name)},
        };

    public SerializedNode Visit(UsingNode node)
        => new SerializedNode(){
            {"type", "UsingNode"},
            {"name", ASTUtils.SerializeUnion(node.Name)},
        };

    public SerializedNode Visit(StructNode node)
        => new SerializedNode(){
            {"type", "StructNode"},
            {"access", node.AccessLevel.ToString()},
            {"name", Serialize(node.Name)},
            {"fields", ASTUtils.SerializeTuple(node.Fields, ASTUtils.SerializeStructField)},
        };

    public SerializedNode Visit(TypeDecName typeDec)
        => new SerializedNode(){
            {"type", "TypeDecName"},
            {"parent", ASTUtils.SerializeValue(typeDec.Parent)},
            {"typename", ASTUtils.SerializeValue(typeDec.TypeName)},
        };

    public SerializedNode Serialize(TypeDecName typeDec) => Visit(typeDec);
    public SerializedNode Serialize(TopLevelNode node) => node.Accept(this);
}