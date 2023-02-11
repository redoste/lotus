using Lotus.Syntax;

namespace Lotus;

public static partial class LotusFacts
{
    internal static bool IsStartOfNumber(char c, char nextChar)
        => Char.IsAsciiDigit(c)
        || (c is '.' && Char.IsAsciiDigit(nextChar));

    public static bool NeedsSemicolon(StatementNode node)
        => node is not (
                ElseNode
            or ForeachNode
            or ForNode
            or FunctionDeclarationNode
            or IfNode
            or WhileNode
        );
}