using System.Collections;
using System.Collections.Immutable;
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

    private ImmutableArray<LuaSyntaxNodeOrToken>? _childrenOrToken;

    private bool _lazyInit;

    public LuaSyntaxNode(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
    {
        Kind = greenNode.IsSyntaxNode ? greenNode.SyntaxKind : LuaSyntaxKind.None;
        GreenNode = greenNode;
        Parent = parent;
        Tree = tree;
        _lazyInit = false;
    }

    private void LazyInit()
    {
        if (!_lazyInit)
        {
            _lazyInit = true;
            _childrenOrToken = GreenNode.Children
                .Select(child => SyntaxFactory.CreateSyntax(child, Tree, this))
                .ToImmutableArray();
        }
    }

    public IEnumerable<LuaSyntaxNode> Children
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            return _childrenOrToken?.Where(nodeOrToken => nodeOrToken.IsNode).Select(nodeOrToken => nodeOrToken.Node!)
                   ?? Enumerable.Empty<LuaSyntaxNode>();
        }
    }

    public IEnumerable<LuaSyntaxNodeOrToken> ChildrenWithTokens
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            return _childrenOrToken ?? Enumerable.Empty<LuaSyntaxNodeOrToken>();
        }
    }

    // 遍历所有后代, 包括自己
    public IEnumerable<LuaSyntaxNode> DescendantsAndSelf()
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        var stack = new Stack<LuaSyntaxNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            foreach (var child in node.Children)
            {
                stack.Push(child);
            }
        }
    }

    // 不包括自己
    public IEnumerable<LuaSyntaxNode> Descendants()
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        var stack = new Stack<LuaSyntaxNode>();
        foreach (var child in Children)
        {
            stack.Push(child);
        }

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            foreach (var child in node.Children)
            {
                stack.Push(child);
            }
        }
    }

    // 遍历所有后代和token, 包括自己
    public IEnumerable<LuaSyntaxNodeOrToken> DescendantsAndSelfWithTokens()
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        var stack = new Stack<LuaSyntaxNodeOrToken>();
        stack.Push(new LuaSyntaxNodeOrToken(this));
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            // ReSharper disable once InvertIf
            if (node.IsNode)
            {
                foreach (var child in node.Node!.ChildrenWithTokens)
                {
                    stack.Push(child);
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


    public T? FirstChild<T>() where T : LuaSyntaxNode
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            return null;
        }

        return (from LuaSyntaxNodeOrToken? child in _childrenOrToken
            where child.IsNode && child.Node is T
            select child.Node as T).FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(LuaTokenKind kind)
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            return null;
        }

        return (from LuaSyntaxNodeOrToken? child in _childrenOrToken
            where child.IsToken && child.Token!.Kind == kind
            select child.Token).FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken()
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            return null;
        }

        return (from LuaSyntaxNodeOrToken? child in _childrenOrToken
            where child.IsToken
            select child.Token).FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(Func<LuaTokenKind, bool> predicate)
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            return null;
        }

        return (from LuaSyntaxNodeOrToken? child in _childrenOrToken
            where child.IsToken && predicate(child.Token!.Kind)
            select child.Token).FirstOrDefault();
    }

    public IEnumerable<T> ChildNodes<T>() where T : LuaSyntaxNode
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            yield break;
        }

        foreach (var child in _childrenOrToken)
        {
            if (child is { IsNode: true, Node: T node })
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaSyntaxToken> ChildTokens(LuaTokenKind kind)
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            yield break;
        }

        foreach (var child in _childrenOrToken)
        {
            if (child is { IsToken: true, Token: { } tk } && tk.Kind == kind)
            {
                yield return tk;
            }
        }
    }
}
