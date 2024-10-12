using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly struct SyntaxIterator(int index, LuaSyntaxTree tree)
{
    public int Index => index;

    private int RawKind => tree.GetRawKind(index);

    private int ParentIndex => tree.GetParent(index);

    private int ChildStartIndex => tree.GetChildStart(index);

    private int ChildFinishIndex => tree.GetChildEnd(index);

    public LuaSyntaxKind Kind => (LuaSyntaxKind)RawKind;

    public LuaTokenKind TokenKind => (LuaTokenKind)RawKind;

    public bool IsNode => tree.IsNode(index);

    public bool IsToken => !IsNode;

    // return true if the iter is valid
    public bool IsValid => index != -1;

    public SyntaxIterator Default => new(-1, tree);

    public LuaPtr<LuaSyntaxElement> ToPtr() => new(UniqueId);

    public LuaPtr<T> ToPtr<T>() where T : LuaSyntaxElement => new(UniqueId);

    public SourceRange Range => tree.GetSourceRange(index);

    public SyntaxElementId UniqueId => new(tree.Document.Id, index);

    public int Position => tree.GetSourceRange(index).StartOffset;

    public IEnumerable<SyntaxIterator> Children
    {
        get
        {
            var start = ChildStartIndex;
            if (start == -1)
            {
                yield break;
            }

            var finish = ChildFinishIndex;
            for (var i = start; i <= finish; i++)
            {
                yield return new SyntaxIterator(i, tree);
            }
        }
    }

    public IEnumerable<SyntaxIterator> ChildrenNodes
    {
        get
        {
            foreach (var child in Children)
            {
                if (child.IsNode)
                {
                    yield return child;
                }
            }
        }
    }

    public IEnumerable<SyntaxIterator> ChildrenTokens
    {
        get
        {
            foreach (var child in Children)
            {
                if (child.IsToken)
                {
                    yield return child;
                }
            }
        }
    }

    public IEnumerable<SyntaxIterator> Descendants
    {
        get
        {
            var stack = new Stack<SyntaxIterator>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                if (node.IsNode)
                {
                    foreach (var child in node.Children.Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    public IEnumerable<SyntaxIterator> DescendantsOfKind(LuaSyntaxKind kind)
    {
        return Descendants.Where(it => it.Kind == kind);
    }

    public IEnumerable<SyntaxIterator> DescendantsInRange(SourceRange range)
    {
        var validChildren = new List<SyntaxIterator>();
        SyntaxIterator parentNode = this;
        var found = false;
        do
        {
            found = false;
            foreach (var child in parentNode.Children)
            {
                if (child.Range.Contain(range))
                {
                    parentNode = child;
                    found = true;
                    break;
                }
            }
        } while (found);

        foreach (var child in parentNode.Children)
        {
            if (child.Range.Intersect(range))
            {
                validChildren.Add(child);
            }
        }

        validChildren.Reverse();
        var stack = new Stack<SyntaxIterator>(validChildren);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Range.Intersect(range))
            {
                yield return node;
            }

            foreach (var child in node.Children.Reverse())
            {
                stack.Push(child);
            }
        }
    }

    public IEnumerable<SyntaxIterator> Ancestors
    {
        get
        {
            var parent = ParentIndex;
            while (parent != -1)
            {
                yield return new SyntaxIterator(parent, tree);
                parent = tree.GetParent(parent);
            }
        }
    }

    public IEnumerable<SyntaxIterator> AncestorsAndSelf
    {
        get
        {
            yield return this;
            var parent = ParentIndex;
            while (parent != -1)
            {
                yield return new SyntaxIterator(parent, tree);
                parent = tree.GetParent(parent);
            }
        }
    }

    public SyntaxIterator Parent => new(ParentIndex, tree);

    public IEnumerable<T> ChildrenNodeOfType<T>(LuaSyntaxKind kind) where T : LuaSyntaxNode
    {
        foreach (var child in ChildrenNodes)
        {
            if (child.Kind == kind && child.ToNode<T>() is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<T> ChildrenNodeOfType<T>(Func<LuaSyntaxKind, bool> predicate) where T : LuaSyntaxNode
    {
        foreach (var child in ChildrenNodes)
        {
            if (predicate(child.Kind) && child.ToNode<T>() is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<T> ChildrenTokenOfType<T>(LuaTokenKind kind) where T : LuaSyntaxToken
    {
        foreach (var child in ChildrenTokens)
        {
            if (child.TokenKind == kind && child.ToToken<T>() is { } token)
            {
                yield return token;
            }
        }
    }

    public SyntaxIterator FirstChildToken(Func<LuaTokenKind, bool> predicate)
    {
        foreach (var child in ChildrenTokens)
        {
            if (predicate(child.TokenKind))
            {
                return child;
            }
        }

        return Default;
    }

    public SyntaxIterator FirstChildToken(LuaTokenKind kind)
    {
        foreach (var child in ChildrenTokens)
        {
            if (child.TokenKind == kind)
            {
                return child;
            }
        }

        return Default;
    }

    public SyntaxIterator FirstChildNode(Func<LuaSyntaxKind, bool> predicate)
    {
        foreach (var child in ChildrenNodes)
        {
            if (predicate(child.Kind))
            {
                return child;
            }
        }

        return Default;
    }

    public SyntaxIterator FirstChildNode(LuaSyntaxKind kind)
    {
        foreach (var child in ChildrenNodes)
        {
            if (child.Kind == kind)
            {
                return child;
            }
        }

        return Default;
    }

    public SyntaxIterator FirstChildToken()
    {
        foreach (var child in ChildrenTokens)
        {
            return child;
        }

        return Default;
    }

    public SyntaxIterator LastChildToken()
    {
        foreach (var child in ChildrenTokens.Reverse())
        {
            return child;
        }

        return Default;
    }

    public SyntaxIterator GetNextSibling(int next = 1)
    {
        var parent = Parent;
        if (!parent.IsValid)
        {
            return Default;
        }

        var start = parent.ChildStartIndex;
        if (start == -1)
        {
            return Default;
        }

        var finish = parent.ChildFinishIndex;
        var nextElementId = index + next;
        return nextElementId <= finish ? new SyntaxIterator(nextElementId, tree) : Default;
    }

    public SyntaxIterator GetPrevSibling(int previous = 1)
    {
        var parent = Parent;
        if (!parent.IsValid)
        {
            return Default;
        }

        var start = parent.ChildStartIndex;
        if (start == -1)
        {
            return Default;
        }

        var nextElementId = index - previous;
        return nextElementId >= start ? new SyntaxIterator(nextElementId, tree) : Default;
    }

    public SyntaxIterator GetPrevToken()
    {
        var prevSibling = GetPrevSibling();
        if (prevSibling.IsToken)
        {
            return prevSibling;
        }

        return prevSibling.LastChildToken();
    }

    public SyntaxIterator GetNextToken()
    {
        var nextSibling = GetNextSibling();
        if (nextSibling.IsToken)
        {
            return nextSibling;
        }

        return nextSibling.FirstChildToken();
    }

    public IEnumerable<SyntaxIterator> PrevOf(Func<SyntaxIterator, bool> predicate)
    {
        var current = this;
        while (true)
        {
            var prev = current.GetPrevSibling();
            if (!prev.IsValid)
            {
                break;
            }

            if (predicate(prev))
            {
                yield return prev;
            }

            current = prev;
        }
    }

    public IEnumerable<SyntaxIterator> NextOf(Func<SyntaxIterator, bool> predicate)
    {
        var current = this;
        while (true)
        {
            var next = current.GetNextSibling();
            if (!next.IsValid)
            {
                break;
            }

            if (predicate(next))
            {
                yield return next;
            }

            current = next;
        }
    }

    // 0 based line and col
    public SyntaxIterator TokenAt(int line, int col)
    {
        var offset = tree.Document.GetOffset(line, col);
        return TokenAt(offset);
    }

    public SyntaxIterator TokenAt(int offset)
    {
        var iterator = this;
        while (iterator.IsValid)
        {
            var child = iterator.Children.FirstOrDefault(it => it.Range.Contain(offset));
            if (child.IsToken)
            {
                return child;
            }

            iterator = child;
        }

        return Default;
    }

    public SyntaxIterator TokenLeftBiasedAt(int line, int col)
    {
        if (col > 0)
        {
            col--;
        }

        var offset = tree.Document.GetOffset(line, col);
        if (offset == tree.Document.Text.Length)
        {
            offset--;
        }

        return offset < 0 ? Default : TokenAt(offset);
    }

    public T? ToNode<T>() where T : LuaSyntaxNode
    {
        return tree.GetElement(index) as T;
    }

    public T? ToToken<T>() where T : LuaSyntaxToken
    {
        return tree.GetElement(index) as T;
    }

    public LuaSyntaxElement? ToElement()
    {
        return tree.GetElement(index);
    }
}
