namespace Lotus.Syntax.Visitors;

using SerializedNode = System.Text.Json.Nodes.JsonObject;

internal sealed class ValueSerializer : IValueVisitor<SerializedNode>
{
    public SerializedNode Default(ValueNode node)
        => node == ValueNode.NULL ?
            new SerializedNode(){{"type", "ValueNodeNULL"}} :
            new SerializedNode(){
                {"type", "ValueNode"},
                {"token", ASTUtils.SerializeToken(node.Token)}
            };

    public SerializedNode Visit(FunctionCallNode node)
        => new SerializedNode(){
            {"type", "FunctionCallNode"},
            {"name", Serialize(node.Name)},
            {"arglist", Serialize(node.ArgList)},
        };

    public SerializedNode Visit(ObjectCreationNode node)
        => new SerializedNode(){
            {"type", "ObjectCreationNode"},
            {"invocation", Serialize(node.Invocation)},
        };

    public SerializedNode Visit(OperationNode node)
        => new SerializedNode(){
            {"type", "OperationNode"},
            {"operationtype", node.OperationType.ToString()},
            {"operands", ASTUtils.SerializeTuple(node.Operands, Serialize)}
        };

    public SerializedNode Visit(TupleNode node)
        => Serialize((Tuple<ValueNode>)node);

    public SerializedNode Visit(ParenthesizedValueNode node)
        => new SerializedNode(){
            {"type", "ParenthesizedValueNode"},
            {"value", Serialize(node.Value)},
        };

    public SerializedNode Visit(NameNode name)
        => new SerializedNode(){
            {"type", "NameNode"},
            {"parts", ASTUtils.SerializeTuple(name.Parts, ASTUtils.SerializeToken)},
        };

    public SerializedNode Visit(NumberNode number)
        => new SerializedNode(){
            {"type", "NumberNode"},
            {"value", number.Value},
        };

    public SerializedNode Visit(StringNode str)
        => new SerializedNode(){
            {"type", "StringNode"},
            {"value", str.Value},
        };

    public SerializedNode Visit(ComplexStringNode complexStr)
        => new SerializedNode(){
            {"type", "ComplexStringNode"},
            {"value", complexStr.Value},
            {"codesections", ASTUtils.SerializeTuple(complexStr.CodeSections, Serialize)},
        };

    public SerializedNode Visit(BoolNode bol)
        => new SerializedNode(){
            {"type", "BoolNode"},
            {"value", bol.Value},
        };

    public SerializedNode Visit(IdentNode ident)
        => new SerializedNode(){
            {"type", "IdentNode"},
            {"value", ident.Value},
        };

    public SerializedNode Visit(FullNameNode fullName)
        => new SerializedNode(){
            {"type", "FullNameNode"},
            {"parts", ASTUtils.SerializeTuple(fullName.Parts, ASTUtils.SerializeToken)},
        };

    public SerializedNode Serialize<TVal>(Tuple<TVal> tuple) where TVal : ValueNode
        => new SerializedNode(){
            {"type", "Tuple"},
            {"items", ASTUtils.SerializeTuple(tuple, Serialize)}
        };

    public SerializedNode Serialize(ValueNode node) => node.Accept(this);
}