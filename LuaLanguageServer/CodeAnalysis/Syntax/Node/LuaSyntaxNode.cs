using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public abstract class LuaSyntaxNode
{
    public LuaSyntaxKind Kind { get; }

    public GreenNode GreenNode { get; }

    public LuaSyntaxNode? Parent { get; }

    public LuaSyntaxTree Tree { get; }

    public LuaSyntaxNode(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
    {
        Kind = greenNode.IsSyntaxNode ? greenNode.SyntaxKind : LuaSyntaxKind.None;
        GreenNode = greenNode;
        Parent = parent;
        Tree = tree;
    }

    public static bool CanCast(GreenNode greenNode)
    {
        return greenNode.IsSyntaxNode;
    }

    // 遍历所有后代, 包括自己
    public IEnumerable<LuaSyntaxNode> DescendantsAndSelf()
    {
        var stack = new Stack<LuaSyntaxNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            foreach (var child in node.GreenNode.Children)
            {
                if (child.IsSyntaxNode)
                {
                    stack.Push(SyntaxFactory.CreateSyntax(child, Tree, node));
                }
            }
        }
    }

    // 不包括自己
    public IEnumerable<LuaSyntaxNode> Descendants()
    {
        var stack = new Stack<LuaSyntaxNode>();
        foreach (var child in GreenNode.Children)
        {
            if (child.IsSyntaxNode)
            {
                stack.Push(SyntaxFactory.CreateSyntax(child, Tree, this));
            }
        }

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            foreach (var child in node.GreenNode.Children)
            {
                if (child.IsSyntaxNode)
                {
                    stack.Push(SyntaxFactory.CreateSyntax(child, Tree, node));
                }
            }
        }
    }

    // 遍历所有后代和token, 包括自己
    public IEnumerable<LuaSyntaxNodeOrToken> DescendantsAndSelfWithTokens()
    {
        var stack = new Stack<LuaSyntaxNodeOrToken>();
        stack.Push(new LuaSyntaxNodeOrToken(this));
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            // ReSharper disable once InvertIf
            if (node.IsNode)
            {
                foreach (var child in node.Node!.GreenNode.Children)
                {
                    stack.Push(new LuaSyntaxNodeOrToken(SyntaxFactory.CreateSyntax(child, Tree, node.Node)));
                }
            }
        }
    }

    // 访问祖先节点
    public IEnumerable<LuaSyntaxNode> Ancestors()
    {
        var parent = Parent;
        while (parent != null)
        {
            yield return parent;
            parent = parent.Parent;
        }
    }

    // 访问祖先节点, 包括自己
    public IEnumerable<LuaSyntaxNode> AncestorsAndSelf()
    {
        var node = this;
        while (node != null)
        {
            yield return node;
            node = node.Parent;
        }
    }

    public T FirstChild<T>() where T : LuaSyntaxNode
    {
        foreach (var child in GreenNode.Children)
        {
            if (T.CanCast(child))
            {
            }
        }
    }
}
