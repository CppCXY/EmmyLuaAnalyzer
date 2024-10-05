using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Visitor;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode(int index, LuaSyntaxTree tree)
    : LuaSyntaxElement(index, tree)
{
    public LuaSyntaxKind Kind => (LuaSyntaxKind)RawKind;

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

    public void VisitSyntaxNode(LuaSyntaxNodeVisitor visitor)
    {
        visitor.Visit(this);
    }

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);
}
