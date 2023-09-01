using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Syntax.Green;


/// <summary>
/// 红绿树中的绿树节点
/// </summary>
public record GreenNode
{
    public SourceRange Range { get; }

    private GreenNode(SourceRange range)
    {
        Range = range;
    }

    public record Node(LuaSyntaxKind Kind, SourceRange Range, List<GreenNode> Children) : GreenNode(Range);
    public record Token(LuaTokenKind Kind, SourceRange Range) : GreenNode(Range);
}

