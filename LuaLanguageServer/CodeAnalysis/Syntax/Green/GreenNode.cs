using System.Collections.Immutable;
using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Green;

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

    public SourceRange Range { get; }

    public int ChildPosition { get; internal set; } = 0;

    private readonly ushort _kind;

    private ImmutableArray<GreenNode>? _children;

    public IEnumerable<GreenNode> Children => IsSyntaxNode ? _children! : ImmutableArray<GreenNode>.Empty;

    private readonly NodeFlags _flag;

    public LuaSyntaxKind SyntaxKind => _flag is NodeFlags.Node ? (LuaSyntaxKind)_kind : LuaSyntaxKind.None;

    public LuaTokenKind TokenKind => _flag is NodeFlags.Token ? (LuaTokenKind)_kind : LuaTokenKind.None;

    public bool IsSyntaxNode => _flag is NodeFlags.Node;

    public bool IsToken => _flag is NodeFlags.Token;

    public GreenNode(LuaSyntaxKind kind, SourceRange range, IEnumerable<GreenNode> children, int childPosition = 0)
    {
        _flag = NodeFlags.Node;
        _kind = (ushort)kind;
        Range = range;
        _children = children.ToImmutableArray();
        ChildPosition = childPosition;
    }

    public GreenNode(LuaTokenKind kind, SourceRange range, int childPosition = 0)
    {
        _flag = NodeFlags.Token;
        _kind = (ushort)kind;
        Range = range;
        ChildPosition = childPosition;
    }
}
