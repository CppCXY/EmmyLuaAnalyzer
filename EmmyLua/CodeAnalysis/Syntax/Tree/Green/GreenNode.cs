using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Green;

/// <summary>
/// 红绿树中的绿树节点
/// </summary>
public class GreenNode
{
    [Flags]
    private enum NodeFlags
    {
        Node = 0x1,
        Token = 0x2
    }

    public int Length { get; }

    private readonly int _kind;

    private readonly List<GreenNode>? _children;

    private NodeFlags Flag => (NodeFlags)(_kind >> 16);

    public ushort RawKind => (ushort)_kind;

    public IEnumerable<GreenNode> Children => IsNode ? _children! : Enumerable.Empty<GreenNode>();

    public LuaSyntaxKind SyntaxKind => Flag == NodeFlags.Node ? (LuaSyntaxKind)RawKind : LuaSyntaxKind.None;

    public LuaTokenKind TokenKind => Flag == NodeFlags.Token ? (LuaTokenKind)RawKind : LuaTokenKind.None;

    public bool IsNode => Flag == NodeFlags.Node;

    public bool IsToken => Flag == NodeFlags.Token;

    public GreenNode(LuaSyntaxKind kind, int length, List<GreenNode>? children)
    {
        Length = length;
        _kind = (ushort)kind | (int)NodeFlags.Node << 16;
        _children = children;
    }

    public GreenNode(LuaTokenKind kind, int length)
    {
        _kind = (ushort)kind | (int)NodeFlags.Token << 16;
        Length = length;
    }
}
