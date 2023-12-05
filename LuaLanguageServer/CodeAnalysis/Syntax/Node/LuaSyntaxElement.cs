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
using LuaLanguageServer.CodeAnalysis.Syntax.Walker;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public abstract class LuaSyntaxElement
{
    public GreenNode Green { get; }

    public LuaSyntaxElement? Parent { get; }

    public LuaSyntaxTree Tree { get; }

    private ImmutableArray<LuaSyntaxElement>? _childrenElements;

    protected bool _lazyInit;

    public LuaSyntaxElement(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        Green = green;
        Parent = parent;
        Tree = tree;
        _lazyInit = false;
    }

    protected void LazyInit()
    {
        if (!_lazyInit)
        {
            _lazyInit = true;
            _childrenElements = Green.Children
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

            return _childrenElements?.OfType<LuaSyntaxNode>()
                   ?? Enumerable.Empty<LuaSyntaxNode>();
        }
    }

    public IEnumerable<LuaSyntaxElement> ChildrenWithTokens
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            return _childrenElements ?? Enumerable.Empty<LuaSyntaxElement>();
        }
    }

    private ImmutableArray<LuaSyntaxElement> ChildrenWithTokenArray
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            return _childrenElements ?? throw new UnreachableException();
        }
    }

    // 遍历所有后代, 包括自己
    public IEnumerable<LuaSyntaxElement> DescendantsAndSelf
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            var stack = new Stack<LuaSyntaxElement>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                foreach (var child in node.Children.Reverse())
                {
                    stack.Push(child);
                }
            }
        }
    }

    // 不包括自己
    public IEnumerable<LuaSyntaxElement> Descendants
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            var stack = new Stack<LuaSyntaxElement>();
            foreach (var child in Children.Reverse())
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                foreach (var child in node.Children.Reverse())
                {
                    stack.Push(child);
                }
            }
        }
    }

    public IEnumerable<LuaSyntaxElement> DescendantsWithToken
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            var stack = new Stack<LuaSyntaxElement>();

            foreach (var child in ChildrenWithTokens.Reverse())
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                // ReSharper disable once InvertIf
                if (node is LuaSyntaxNode n)
                {
                    foreach (var child in n.ChildrenWithTokens.Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    public void Accept(ILuaNodeWalker walker)
    {
        if (this is LuaSyntaxNode node)
        {
            walker.WalkIn(node);
            foreach (var child in Children)
            {
                child.Accept(walker);
            }

            walker.WalkOut(node);
        }
    }

    public void Accept(ILuaElementWalker walker)
    {
        walker.WalkIn(this);
        foreach (var child in Children)
        {
            child.Accept(walker);
        }

        walker.WalkOut(this);
    }

    // 遍历所有后代和token, 包括自己
    public IEnumerable<LuaSyntaxElement> DescendantsAndSelfWithTokens
    {
        get
        {
            if (!_lazyInit)
            {
                LazyInit();
            }

            var stack = new Stack<LuaSyntaxElement>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                // ReSharper disable once InvertIf
                if (node is LuaSyntaxNode n)
                {
                    foreach (var child in n.ChildrenWithTokens.Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    // 访问祖先节点
    public IEnumerable<LuaSyntaxElement> Ancestors
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
    public IEnumerable<LuaSyntaxElement> AncestorsAndSelf
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


    public T? FirstChild<T>() where T : LuaSyntaxElement
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        return _childrenElements == null ? null : _childrenElements.OfType<T>().FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(LuaTokenKind kind)
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        return _childrenElements == null
            ? null
            : _childrenElements.OfType<LuaSyntaxToken>().FirstOrDefault(it => it.Kind == kind);
    }

    public LuaSyntaxToken? FirstChildToken()
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        return _childrenElements == null ? null : _childrenElements.OfType<LuaSyntaxToken>().FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(Func<LuaTokenKind, bool> predicate)
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        return _childrenElements == null
            ? null
            : _childrenElements.OfType<LuaSyntaxToken>().FirstOrDefault(it => predicate(it.Kind));
    }

    public IEnumerable<T> ChildNodes<T>() where T : LuaSyntaxElement
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        return _childrenElements?.OfType<T>() ?? Enumerable.Empty<T>();
    }

    public IEnumerable<T> ChildNodesBeforeToken<T>(LuaTokenKind kind) where T : LuaSyntaxElement
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenElements == null)
        {
            yield break;
        }

        foreach (var child in _childrenElements)
        {
            switch (child)
            {
                case LuaSyntaxToken token when token.Kind == kind:
                    yield break;
                case T node:
                    yield return node;
                    break;
            }
        }
    }

    public IEnumerable<T> ChildNodesAfterToken<T>(LuaTokenKind kind) where T : LuaSyntaxElement
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenElements == null)
        {
            yield break;
        }

        var afterToken = false;
        foreach (var child in _childrenElements)
        {
            if (afterToken && child is T node)
            {
                yield return node;
            }

            if (child is LuaSyntaxToken token && token.Kind == kind)
            {
                afterToken = true;
            }
        }
    }

    public T? ChildNodeAfterToken<T>(LuaTokenKind kind) where T : LuaSyntaxElement
    {
        if (!_lazyInit)
        {
            LazyInit();
        }

        if (_childrenElements == null)
        {
            return null!;
        }

        var afterToken = false;
        foreach (var child in _childrenElements)
        {
            if (afterToken && child is T node)
            {
                return node;
            }

            if (child is LuaSyntaxToken token && token.Kind == kind)
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

        if (_childrenElements == null)
        {
            yield break;
        }

        foreach (var child in _childrenElements)
        {
            if (child is LuaSyntaxToken token && token.Kind == kind)
            {
                yield return token;
            }
        }
    }

    public string DebugSyntaxInspect()
    {
        var sb = new StringBuilder();
        var stack = new Stack<(LuaSyntaxElement node, int level)>();

        stack.Push((this, 0));
        while (stack.Count > 0)
        {
            var (nodeOrToken, level) = stack.Pop();
            sb.Append(' ', level * 2);
            switch (nodeOrToken)
            {
                case LuaSyntaxNode node:
                {
                    sb.AppendLine(
                        $"{node.GetType().Name}@[{node.Green.Range.StartOffset}..{node.Green.Range.StartOffset + node.Green.Range.Length})");
                    foreach (var child in node.ChildrenWithTokens.Reverse())
                    {
                        stack.Push((child, level + 1));
                    }

                    break;
                }
                case LuaSyntaxToken token:
                {
                    var detail = token.Kind switch
                    {
                        LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia => "",
                        _ => $"\"{token.Text}\""
                    };

                    sb.AppendLine(
                        $"{token.Kind}@[{token.Green.Range.StartOffset}..{token.Green.Range.StartOffset + token.Green.Range.Length}) {detail}");
                    break;
                }
            }
        }

        return sb.ToString();
    }

    public string DebugGreenInspect()
    {
        var sb = new StringBuilder();
        var stack = new Stack<(LuaSyntaxElement node, int level)>();

        stack.Push((this, 0));
        while (stack.Count > 0)
        {
            var (luaSyntaxElement, level) = stack.Pop();
            sb.Append(' ', level * 2);
            switch (luaSyntaxElement)
            {
                case LuaSyntaxNode node:
                {
                    sb.AppendLine(
                        $"{node.Kind}@[{node.Green.Range.StartOffset}..{node.Green.Range.StartOffset + node.Green.Range.Length})");
                    foreach (var child in node.ChildrenWithTokens.Reverse())
                    {
                        stack.Push((child, level + 1));
                    }

                    break;
                }
                case LuaSyntaxToken token:
                {
                    var detail = token.Kind switch
                    {
                        LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia => "",
                        _ => $"\"{token.Text}\""
                    };

                    sb.AppendLine(
                        $"{token.Kind}@[{token.Green.Range.StartOffset}..{token.Green.Range.StartOffset + token.Green.Range.Length}) {detail}");
                    break;
                }
            }
        }

        return sb.ToString();
    }

    public LuaSyntaxElement? GetNextSibling(int next = 1) =>
        Parent?.ChildrenWithTokens.ElementAtOrDefault(Green.ChildPosition + next);

    public LuaSyntaxElement? GetPrevSibling(int prev = 1) =>
        Parent?.ChildrenWithTokens.ElementAtOrDefault(Green.ChildPosition - prev);

    // 从自身向前迭代, 直到找到一个类型为T的节点
    public T? PrevOfType<T>()
        where T : LuaSyntaxElement
    {
        if (Parent?.ChildrenWithTokenArray is { } childrenWithTokenArray)
        {
            var selfPosition = Green.ChildPosition;
            for (var i = selfPosition - 1; i >= 0; i--)
            {
                var nodeOrToken = childrenWithTokenArray[i];
                if (nodeOrToken is T node)
                {
                    return node;
                }
            }
        }

        return null;
    }

    public LuaSourceLocation Location => new LuaSourceLocation(Tree, Green.Range);

    public LuaSyntaxToken TokenAt(int offset)
    {
        var node = this;
        while (node != null)
        {
            var nodeElement = node.ChildrenWithTokens.FirstOrDefault(it => it.Green.Range.Contain(offset));
            if (nodeElement is LuaSyntaxToken token)
            {
                return token;
            }

            node = nodeElement;
        }

        throw new ArgumentOutOfRangeException();
    }

    // 0 based line and col
    public LuaSyntaxToken TokenAt(int line, int col)
    {
        var offset = Tree.Source.GetOffset(line, col);
        return TokenAt(offset);
    }

    public LuaSyntaxNode NodeAt(int line, int col)
    {
        var token = TokenAt(line, col);
        return (token.Parent as LuaSyntaxNode)!;
    }

    public override int GetHashCode()
    {
        return Green.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is LuaSyntaxElement other && Green.Equals(other.Green);
    }
}
