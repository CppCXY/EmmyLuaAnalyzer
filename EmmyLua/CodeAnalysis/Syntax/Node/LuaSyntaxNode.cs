using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode(int index, LuaSyntaxTree tree)
    : LuaSyntaxElement(index, tree)
{
    public LuaSyntaxKind Kind => (LuaSyntaxKind)RawKind;

    private List<LuaSyntaxElement>? _children = null;

    protected override List<LuaSyntaxElement> ChildrenElements
    {
        get
        {
            if (_children == null)
            {
                _children = new List<LuaSyntaxElement>();
                if (ChildStartIndex == -1 || ChildFinishIndex == -1)
                {
                    return _children;
                }

                for(var i = ChildStartIndex; i <= ChildFinishIndex; i++)
                {
                    _children.Add(Tree.GetElement(i));
                }
            }

            return _children;
        }
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsAndSelf
    {
        get
        {
            var stack = new Stack<LuaSyntaxElement>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                foreach (var child in node.ChildrenNode.Reverse())
                {
                    stack.Push(child);
                }
            }
        }
    }

    public override IEnumerable<LuaSyntaxNode> Descendants
    {
        get
        {
            var stack = new Stack<LuaSyntaxNode>();
            foreach (var child in ChildrenNode.Reverse())
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                foreach (var child in node.ChildrenNode.Reverse())
                {
                    stack.Push(child);
                }
            }
        }
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsInRange(SourceRange range)
    {
        var validChildren = new List<LuaSyntaxElement>();
        LuaSyntaxElement parentNode = this;
        var found = false;
        do
        {
            found = false;
            foreach (var child in parentNode.ChildrenWithTokens)
            {
                if (child.Range.Contain(range))
                {
                    parentNode = child;
                    found = true;
                    break;
                }
            }
        } while (found);

        foreach (var child in parentNode.ChildrenWithTokens)
        {
            if (child.Range.Intersect(range))
            {
                validChildren.Add(child);
            }
        }

        validChildren.Reverse();
        var stack = new Stack<LuaSyntaxElement>(validChildren);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Range.Intersect(range))
            {
                yield return node;
            }

            foreach (var child in node.ChildrenNode.Reverse())
            {
                stack.Push(child);
            }
        }
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsWithToken
    {
        get
        {
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

    public override IEnumerable<LuaSyntaxElement> DescendantsAndSelfWithTokens
    {
        get
        {
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

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);
}
