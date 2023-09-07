using System.Collections;
using System.Collections.Immutable;
using System.Text;
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

    public IEnumerable<T> ChildNodesBeforeToken<T>(LuaTokenKind kind) where T : LuaSyntaxNode
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
            switch (child)
            {
                case { IsToken: true, Token: { } tk } when tk.Kind == kind:
                    yield break;
                case { IsNode: true, Node: T node }:
                    yield return node;
                    break;
            }
        }
    }

    public IEnumerable<T> ChildNodesAfterToken<T>(LuaTokenKind kind) where T : LuaSyntaxNode
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            yield break;
        }

        var afterToken = false;
        foreach (var child in _childrenOrToken)
        {
            if (afterToken && child is { IsNode: true, Node: T node })
            {
                yield return node;
            }

            if (child is { IsToken: true, Token: { } tk } && tk.Kind == kind)
            {
                afterToken = true;
            }
        }
    }

    public T? ChildNodeAfterToken<T>(LuaTokenKind kind) where T : LuaSyntaxNode
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenOrToken == null)
        {
            return null!;
        }

        var afterToken = false;
        foreach (var child in _childrenOrToken)
        {
            if (afterToken && child is { IsNode: true, Node: T node })
            {
                return node;
            }

            if (child is { IsToken: true, Token: { } tk } && tk.Kind == kind)
            {
                afterToken = true;
            }
        }

        return null;
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

    public string DebugSyntaxInspect()
    {
        var sb = new StringBuilder();
        var stack = new Stack<(LuaSyntaxNodeOrToken node, int level)>();

        stack.Push((new LuaSyntaxNodeOrToken(this), 0));
        while (stack.Count > 0)
        {
            var (nodeOrToken, level) = stack.Pop();
            sb.Append(' ', level * 2);
            if (nodeOrToken.IsNode)
            {
                var node = nodeOrToken.Node!;
                sb.AppendLine(
                    $"{node.GetType().Name}@[{node.GreenNode.Range.StartOffset}..{node.GreenNode.Range.StartOffset + node.GreenNode.Range.Length})");
                foreach (var child in node.ChildrenWithTokens.Reverse())
                {
                    stack.Push((child, level + 1));
                }
            }
            else
            {
                var token = nodeOrToken.Token!;
                var detail = token.Kind switch
                {
                    LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia => "",
                    _ => $"\"{token.Text}\""
                };

                sb.AppendLine(
                    $"{token.Kind}@[{token.GreenNode.Range.StartOffset}..{token.GreenNode.Range.StartOffset + token.GreenNode.Range.Length}) {detail}");
            }
        }

        return sb.ToString();
    }

    public string DebugGreenInspect()
    {
        var sb = new StringBuilder();
        var stack = new Stack<(LuaSyntaxNodeOrToken node, int level)>();

        stack.Push((new LuaSyntaxNodeOrToken(this), 0));
        while (stack.Count > 0)
        {
            var (nodeOrToken, level) = stack.Pop();
            sb.Append(' ', level * 2);
            if (nodeOrToken.IsNode)
            {
                var node = nodeOrToken.Node!;
                sb.AppendLine(
                    $"{node.Kind}@[{node.GreenNode.Range.StartOffset}..{node.GreenNode.Range.StartOffset + node.GreenNode.Range.Length})");
                foreach (var child in node.ChildrenWithTokens.Reverse())
                {
                    stack.Push((child, level + 1));
                }
            }
            else
            {
                var token = nodeOrToken.Token!;
                var detail = token.Kind switch
                {
                    LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia => "",
                    _ => $"\"{token.Text}\""
                };

                sb.AppendLine(
                    $"{token.Kind}@[{token.GreenNode.Range.StartOffset}..{token.GreenNode.Range.StartOffset + token.GreenNode.Range.Length}) {detail}");
            }
        }

        return sb.ToString();
    }
}
