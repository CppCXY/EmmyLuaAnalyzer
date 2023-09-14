using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
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

            return _childrenOrToken?
                       .Where(nodeOrToken => nodeOrToken is LuaSyntaxNodeOrToken.Node)
                       .Select(nodeOrToken => (nodeOrToken as LuaSyntaxNodeOrToken.Node)!.SyntaxNode)
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

    private ImmutableArray<LuaSyntaxNodeOrToken> ChildrenWithTokenArray
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            return _childrenOrToken ?? throw new UnreachableException();
        }
    }

    // 遍历所有后代, 包括自己
    public IEnumerable<LuaSyntaxNode> DescendantsAndSelf
    {
        get
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
    }

    // 不包括自己
    public IEnumerable<LuaSyntaxNode> Descendants
    {
        get
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
    }

    public IEnumerable<LuaSyntaxNodeOrToken> DescendantsWithToken
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            var stack = new Stack<LuaSyntaxNodeOrToken>();

            foreach (var child in ChildrenWithTokens)
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                // ReSharper disable once InvertIf
                if (node is LuaSyntaxNodeOrToken.Node n)
                {
                    foreach (var child in n.SyntaxNode.ChildrenWithTokens)
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    // 遍历所有后代和token, 包括自己
    public IEnumerable<LuaSyntaxNodeOrToken> DescendantsAndSelfWithTokens
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            var stack = new Stack<LuaSyntaxNodeOrToken>();
            stack.Push(new LuaSyntaxNodeOrToken.Node(this));
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                // ReSharper disable once InvertIf
                if (node is LuaSyntaxNodeOrToken.Node n)
                {
                    foreach (var child in n.SyntaxNode.ChildrenWithTokens)
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    // 访问祖先节点
    public IEnumerable<LuaSyntaxNode> Ancestors
    {
        get
        {
            var parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }
    }

    // 访问祖先节点, 包括自己
    public IEnumerable<LuaSyntaxNode> AncestorsAndSelf
    {
        get
        {
            var node = this;
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
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
            where child is LuaSyntaxNodeOrToken.Node { SyntaxNode: T }
            select (child as LuaSyntaxNodeOrToken.Node).SyntaxNode as T).FirstOrDefault();
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
            where child is LuaSyntaxNodeOrToken.Token token && token.SyntaxToken.Kind == kind
            select (child as LuaSyntaxNodeOrToken.Token).SyntaxToken).FirstOrDefault();
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
            where child is LuaSyntaxNodeOrToken.Token
            select (child as LuaSyntaxNodeOrToken.Token).SyntaxToken).FirstOrDefault();
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
            where child is LuaSyntaxNodeOrToken.Token token && predicate(token.SyntaxToken.Kind)
            select (child as LuaSyntaxNodeOrToken.Token).SyntaxToken).FirstOrDefault();
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
            if (child is LuaSyntaxNodeOrToken.Node { SyntaxNode: T node })
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
                case LuaSyntaxNodeOrToken.Token token when token.SyntaxToken.Kind == kind:
                    yield break;
                case LuaSyntaxNodeOrToken.Node { SyntaxNode: T node }:
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
            if (afterToken && child is LuaSyntaxNodeOrToken.Node { SyntaxNode: T node })
            {
                yield return node;
            }

            if (child is LuaSyntaxNodeOrToken.Token token && token.SyntaxToken.Kind == kind)
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
            if (afterToken && child is LuaSyntaxNodeOrToken.Node { SyntaxNode: T node })
            {
                return node;
            }

            if (child is LuaSyntaxNodeOrToken.Token token && token.SyntaxToken.Kind == kind)
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
            if (child is LuaSyntaxNodeOrToken.Token token && token.SyntaxToken.Kind == kind)
            {
                yield return token.SyntaxToken;
            }
        }
    }

    public string DebugSyntaxInspect()
    {
        var sb = new StringBuilder();
        var stack = new Stack<(LuaSyntaxNodeOrToken node, int level)>();

        stack.Push((new LuaSyntaxNodeOrToken.Node(this), 0));
        while (stack.Count > 0)
        {
            var (nodeOrToken, level) = stack.Pop();
            sb.Append(' ', level * 2);
            switch (nodeOrToken)
            {
                case LuaSyntaxNodeOrToken.Node { SyntaxNode: { } node }:
                {
                    sb.AppendLine(
                        $"{node.GetType().Name}@[{node.GreenNode.Range.StartOffset}..{node.GreenNode.Range.StartOffset + node.GreenNode.Range.Length})");
                    foreach (var child in node.ChildrenWithTokens.Reverse())
                    {
                        stack.Push((child, level + 1));
                    }

                    break;
                }
                case LuaSyntaxNodeOrToken.Token { SyntaxToken: { } token }:
                {
                    var detail = token.Kind switch
                    {
                        LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia => "",
                        _ => $"\"{token.Text}\""
                    };

                    sb.AppendLine(
                        $"{token.Kind}@[{token.GreenNode.Range.StartOffset}..{token.GreenNode.Range.StartOffset + token.GreenNode.Range.Length}) {detail}");
                    break;
                }
            }
        }

        return sb.ToString();
    }

    public string DebugGreenInspect()
    {
        var sb = new StringBuilder();
        var stack = new Stack<(LuaSyntaxNodeOrToken node, int level)>();

        stack.Push((new LuaSyntaxNodeOrToken.Node(this), 0));
        while (stack.Count > 0)
        {
            var (nodeOrToken, level) = stack.Pop();
            sb.Append(' ', level * 2);
            switch (nodeOrToken)
            {
                case LuaSyntaxNodeOrToken.Node { SyntaxNode: { } node }:
                {
                    sb.AppendLine(
                        $"{node.Kind}@[{node.GreenNode.Range.StartOffset}..{node.GreenNode.Range.StartOffset + node.GreenNode.Range.Length})");
                    foreach (var child in node.ChildrenWithTokens.Reverse())
                    {
                        stack.Push((child, level + 1));
                    }

                    break;
                }
                case LuaSyntaxNodeOrToken.Token { SyntaxToken: { } token }:
                {
                    var detail = token.Kind switch
                    {
                        LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia => "",
                        _ => $"\"{token.Text}\""
                    };

                    sb.AppendLine(
                        $"{token.Kind}@[{token.GreenNode.Range.StartOffset}..{token.GreenNode.Range.StartOffset + token.GreenNode.Range.Length}) {detail}");
                    break;
                }
            }
        }

        return sb.ToString();
    }

    public LuaSyntaxNodeOrToken? GetNextSibling(int next = 1) =>
        Parent?.ChildrenWithTokens.ElementAtOrDefault(GreenNode.ChildPosition + next);

    public LuaSyntaxNodeOrToken? GetPrevSibling(int prev = 1) =>
        Parent?.ChildrenWithTokens.ElementAtOrDefault(GreenNode.ChildPosition - prev);

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(new LuaSyntaxNodeOrToken.Node(this)) ?? Enumerable.Empty<LuaCommentSyntax>();

    // 从自身向前迭代, 直到找到一个类型为T的节点
    public T? PrevOfType<T>()
        where T : LuaSyntaxNode
    {
        if (Parent?.ChildrenWithTokenArray is { } childrenWithTokenArray)
        {
            var selfPosition = GreenNode.ChildPosition;
            for (var i = selfPosition - 1; i >= 0; i--)
            {
                var nodeOrToken = childrenWithTokenArray[i];
                if (nodeOrToken is LuaSyntaxNodeOrToken.Node { SyntaxNode: T node })
                {
                    return node;
                }
            }
        }

        return null;
    }

    public LuaSourceLocation Location => new LuaSourceLocation(Tree, GreenNode.Range);

    public LuaSyntaxToken TokenAt(int offset)
    {
        var node = this;
        while (node != null)
        {
            var nodeOrToken = node.ChildrenWithTokens.FirstOrDefault(it => it switch
            {
                LuaSyntaxNodeOrToken.Node n => n.SyntaxNode.GreenNode.Range.Contain(offset),
                LuaSyntaxNodeOrToken.Token t => t.SyntaxToken.GreenNode.Range.Contain(offset),
                _ => throw new UnreachableException()
            });
            if (nodeOrToken is LuaSyntaxNodeOrToken.Token { SyntaxToken: { } token })
            {
                return token;
            }

            node = nodeOrToken switch
            {
                LuaSyntaxNodeOrToken.Node n => n.SyntaxNode,
                _ => null
            };
        }

        throw new ArgumentOutOfRangeException();
    }

    public LuaSyntaxToken TokenAt(int line, int col)
    {
        var offset = Tree.Source.GetOffset(line, col);
        return TokenAt(offset);
    }

    public override int GetHashCode()
    {
        return GreenNode.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is LuaSyntaxNode other && GreenNode.Equals(other.GreenNode);
    }
}
