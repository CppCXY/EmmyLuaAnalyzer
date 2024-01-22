using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Green;

/// <summary>
/// 红绿树中的绿树节点
/// </summary>
public class GreenNode
{
    private enum NodeFlags
    {
        Node,
        Token
    }

    public int Length { get; }

    private readonly ushort _kind;

    private List<GreenNode>? _children;

    public IEnumerable<GreenNode> Children => IsNode ? _children! : Enumerable.Empty<GreenNode>();

    private readonly NodeFlags _flag;

    public LuaSyntaxKind SyntaxKind => _flag is NodeFlags.Node ? (LuaSyntaxKind)_kind : LuaSyntaxKind.None;

    public LuaTokenKind TokenKind => _flag is NodeFlags.Token ? (LuaTokenKind)_kind : LuaTokenKind.None;

    public bool IsNode => _flag is NodeFlags.Node;

    public bool IsToken => _flag is NodeFlags.Token;

    public GreenNode(LuaSyntaxKind kind, int length, IEnumerable<GreenNode> children)
    {
        _flag = NodeFlags.Node;
        _kind = (ushort)kind;
        Length = length;
        _children = children.ToList();
    }

    public GreenNode(LuaTokenKind kind,  int length)
    {
        _flag = NodeFlags.Token;
        _kind = (ushort)kind;
        Length = length;
    }

    public GreenNode With(int length)
    {
        return _flag is NodeFlags.Node ? new GreenNode(SyntaxKind, length, Children) : new GreenNode(TokenKind, length);
    }
}
