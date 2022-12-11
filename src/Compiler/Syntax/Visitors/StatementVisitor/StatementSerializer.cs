namespace Lotus.Syntax.Visitors;

using SerializedNode = System.Text.Json.Nodes.JsonObject;

internal sealed class StatementSerializer : IStatementVisitor<SerializedNode>
{
    public SerializedNode Default(StatementNode node)
        => node == StatementNode.NULL ?
            new SerializedNode(){{"type", "StatementNodeNULL"}} :
            new SerializedNode(){
                {"type", "StatementNode"},
                {"token", ASTUtils.SerializeToken(node.Token)}
            };

    public SerializedNode Visit(DeclarationNode node)
        => new SerializedNode(){
            {"type", "DeclarationNode"},
            {"name", ASTUtils.SerializeToken(node.Name)},
            {"value", ASTUtils.SerializeValue(node.Value)},
        };

    public SerializedNode Visit(ElseNode node)
        => new SerializedNode(){
            {"type", "ElseNode"},
            {"body", Serialize(node.Body)},
        };

    public SerializedNode Visit(ForeachNode node)
        => new SerializedNode(){
            {"type", "ForeachNode"},
            {"itemname", ASTUtils.SerializeValue(node.ItemName)},
            {"collectionref", ASTUtils.SerializeValue(node.CollectionRef)},
            {"body", Serialize(node.Body)},
        };

    public SerializedNode Visit(ForNode node)
        => new SerializedNode(){
            {"type", "ForNode"},
            {"header", ASTUtils.SerializeTuple(node.Header, Serialize)},
            {"body", Serialize(node.Body)},
        };

    public SerializedNode Visit(FunctionDeclarationNode node)
        => new SerializedNode(){
            {"type", "FunctionDeclarationNode"},
            {"funcname", ASTUtils.SerializeToken(node.FuncName)},
            {"paramlist", ASTUtils.SerializeTuple(node.ParamList, ASTUtils.SerializeFunctionParameter)},
            {"returntype", ASTUtils.SerializeValue(node.ReturnType)},
            {"body", Serialize(node.Body)},
        };

    public SerializedNode Visit(IfNode node)
        => new SerializedNode(){
            {"type", "IfNode"},
            {"condition", ASTUtils.SerializeValue(node.Condition)},
            {"body", Serialize(node.Body)},
            {"else", Serialize(node.ElseNode)}
        };

    public SerializedNode Visit(PrintNode node)
        => new SerializedNode(){
            {"type", "PrintNode"},
            {"value", ASTUtils.SerializeValue(node.Value)},
        };

    public SerializedNode Visit(ReturnNode node)
        => new SerializedNode(){
            {"type", "ReturnNode"},
            {"value", ASTUtils.SerializeValue(node.Value)}
        };

    public SerializedNode Visit(BreakNode node)
        => new SerializedNode(){
            {"type", "BreakNode"},
        };

    public SerializedNode Visit(ContinueNode node)
        => new SerializedNode(){
            {"type", "ContinueNode"},
        };

    public SerializedNode Visit(StatementExpressionNode node)
        => new SerializedNode(){
            {"type", "StatementExpressionNode"},
            {"value", ASTUtils.SerializeValue(node.Value)},
        };

    public SerializedNode Visit(WhileNode node)
        => new SerializedNode(){
            {"type", "WhileNode"},
            {"isdoloop", node.IsDoLoop},
            {"condition", ASTUtils.SerializeValue(node.Condition)},
            {"body", Serialize(node.Body)},
        };

    public SerializedNode Serialize(StatementNode node) => node.Accept(this);

    public SerializedNode Serialize(Tuple<StatementNode> tuple)
        => new SerializedNode(){
            {"type", "Tuple"},
            {"items", ASTUtils.SerializeTuple(tuple, Serialize)},
        };
}