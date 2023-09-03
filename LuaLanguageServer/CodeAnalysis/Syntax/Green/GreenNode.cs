using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Green;

/// <summary>
/// 红绿树中的绿树节点
/// </summary>
public class GreenNode
{
    enum NodeFlags
    {
        Node,
        Token
    }

    public SourceRange Range { get; }

    public int Slot { get; internal set; } = 0;

    private readonly ushort _kind;

    private List<GreenNode>? _children;

    public List<GreenNode> Children => IsSyntaxNode ? _children! : new List<GreenNode>();

    private readonly NodeFlags _flag;

    public LuaSyntaxKind SyntaxKind => _flag is NodeFlags.Node ? (LuaSyntaxKind)_kind : LuaSyntaxKind.None;

    public LuaTokenKind TokenKind => _flag is NodeFlags.Token ? (LuaTokenKind)_kind : LuaTokenKind.None;

    public bool IsSyntaxNode => _flag is NodeFlags.Node;

    public bool IsToken => _flag is NodeFlags.Token;

    public GreenNode(LuaSyntaxKind kind, SourceRange range, List<GreenNode> children)
    {
        _flag = NodeFlags.Node;
        _kind = (ushort)kind;
        Range = range;
        _children = children;
    }

    public GreenNode(LuaTokenKind kind, SourceRange range)
    {
        _flag = NodeFlags.Token;
        _kind = (ushort)kind;
        Range = range;
    }

    // 遍历所有后代, 包括自己
    public IEnumerable<GreenNode> DescendantWithSelf()
    {
        var stack = new Stack<GreenNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            if (node.IsSyntaxNode)
            {
                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }
        }
    }

    // 不包括自己
    public IEnumerable<GreenNode> Descendants()
    {
        var stack = new Stack<GreenNode>();
        if (IsSyntaxNode)
        {
            foreach (var child in Children)
            {
                stack.Push(child);
            }
        }

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            if (node.IsSyntaxNode)
            {
                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }
        }
    }
}
