using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public abstract class LuaSyntaxElement(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : IEquatable<LuaSyntaxElement>
{
    protected int RawKind { get; } = green.RawKind;

    private int ParentIndex { get; set; } = parent?.ElementId ?? -1;

    private int PreviousSiblingIndex { get; set; } = -1;

    private int NextSiblingIndex { get; set; } = -1;

    private int ChildStartIndex { get; set; } = -1;

    private int ChildFinishIndex { get; set; } = -1;

    public LuaSyntaxElement? Parent => Tree.GetElement(ParentIndex);

    public LuaSyntaxTree Tree { get; } = tree;

    public LuaDocumentId DocumentId => Tree.Document.Id;

    public SourceRange Range { get; } = new(startOffset, green.Length);

    public int ElementId { get; internal set; }

    // public int ChildPosition { get; internal set; } = 0;

    public IEnumerable<LuaSyntaxElement> ChildrenElements
    {
        get
        {
            if (ChildStartIndex == -1)
            {
                yield break;
            }

            var index = ChildStartIndex;
            while (index != -1)
            {
                var element = Tree.GetElement(index)!;
                yield return element;
                index = element.NextSiblingIndex;
            }
        }
    }

    public IEnumerable<LuaSyntaxNode> ChildrenNode => ChildrenElements.OfType<LuaSyntaxNode>();

    public IEnumerable<LuaSyntaxElement> ChildrenWithTokens => ChildrenElements;

    public void AddChild(LuaSyntaxElement child)
    {
        if (ChildStartIndex == -1)
        {
            ChildStartIndex = child.ElementId;
        }

        var sibling = ChildFinishIndex;
        ChildFinishIndex = child.ElementId;
        if (sibling != -1)
        {
            Tree.GetElement(sibling)!.NextSiblingIndex = child.ElementId;
            child.PreviousSiblingIndex = sibling;
        }
    }

    // 遍历所有后代, 包括自己
    public IEnumerable<LuaSyntaxElement> DescendantsAndSelf
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

    // 不包括自己
    public IEnumerable<LuaSyntaxElement> Descendants
    {
        get
        {
            var stack = new Stack<LuaSyntaxElement>();
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

    public IEnumerable<LuaSyntaxElement> DescendantsInRange(SourceRange range)
    {
        var validChildren = new List<LuaSyntaxElement>();
        var parentNode = this;
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

    public IEnumerable<LuaSyntaxElement> DescendantsWithToken
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

    public void Accept(ILuaNodeWalker walker)
    {
        if (this is LuaSyntaxNode node)
        {
            walker.WalkIn(node);
            foreach (var child in ChildrenNode)
            {
                child.Accept(walker);
            }

            walker.WalkOut(node);
        }
    }

    public void Accept(ILuaElementWalker walker)
    {
        walker.WalkIn(this);
        foreach (var child in ChildrenNode)
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
        return ChildrenElements == null ? null : ChildrenElements.OfType<T>().FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(LuaTokenKind kind)
    {
        return ChildrenElements == null
            ? null
            : ChildrenElements.OfType<LuaSyntaxToken>().FirstOrDefault(it => it.Kind == kind);
    }

    public LuaSyntaxToken? FirstChildToken()
    {
        return ChildrenElements == null ? null : ChildrenElements.OfType<LuaSyntaxToken>().FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(Func<LuaTokenKind, bool> predicate)
    {
        return ChildrenElements == null
            ? null
            : ChildrenElements.OfType<LuaSyntaxToken>().FirstOrDefault(it => predicate(it.Kind));
    }

    public IEnumerable<T> ChildNodes<T>() where T : LuaSyntaxElement =>
        ChildrenElements?.OfType<T>() ?? Enumerable.Empty<T>();

    public IEnumerable<T> ChildNodesBeforeToken<T>(LuaTokenKind kind) where T : LuaSyntaxElement
    {
        if (ChildrenElements == null)
        {
            yield break;
        }

        foreach (var child in ChildrenElements)
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
        if (ChildrenElements == null)
        {
            yield break;
        }

        var afterToken = false;
        foreach (var child in ChildrenElements)
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
        if (ChildrenElements == null)
        {
            return null!;
        }

        var afterToken = false;
        foreach (var child in ChildrenElements)
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
        if (ChildrenElements == null)
        {
            yield break;
        }

        foreach (var child in ChildrenElements)
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
                        $"{node.GetType().Name}@[{node.Range.StartOffset}..{node.Range.StartOffset + node.Range.Length})");
                    foreach (var child in node.ChildrenWithTokens.Reverse())
                    {
                        stack.Push((child, level + 1));
                    }

                    break;
                }
                case LuaSyntaxToken token:
                {
                    var detail = token switch
                    {
                        {
                            Kind: LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkDocTrivia
                        } => "",
                        LuaStringToken stringToken => $"\"{stringToken.Value}\"",
                        LuaIntegerToken integerToken => $"{integerToken.Value}",
                        LuaFloatToken floatToken => $"{floatToken.Value}",
                        LuaNameToken nameToken => $"{nameToken.RepresentText}",
                        _ => $"\"{token.Text}\""
                    };

                    sb.AppendLine(
                        $"{token.Kind}@[{token.Range.StartOffset}..{token.Range.StartOffset + token.Range.Length}) {detail}");
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
                        $"{node.Kind}@[{node.Range.StartOffset}..{node.Range.StartOffset + node.Range.Length})");
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
                        $"{token.Kind}@[{token.Range.StartOffset}..{token.Range.StartOffset + token.Range.Length}) {detail}");
                    break;
                }
            }
        }

        return sb.ToString();
    }

    public LuaSyntaxElement? GetNextSibling(int next = 1)
    {
        var index = NextSiblingIndex;
        while (index != -1 && next > 0)
        {
            var element = Tree.GetElement(index)!;
            index = element.NextSiblingIndex;
            next--;
        }

        if (next == 0)
        {
            return Tree.GetElement(index);
        }
        else
        {
            return null;
        }
    }

    public LuaSyntaxElement? GetPrevSibling(int prev = 1)
    {
        var index = PreviousSiblingIndex;
        while (index != -1 && prev > 0)
        {
            var element = Tree.GetElement(index)!;
            index = element.PreviousSiblingIndex;
            prev--;
        }

        if (prev == 0)
        {
            return Tree.GetElement(index);
        }
        else
        {
            return null;
        }
    }

    public LuaSyntaxToken? GetPrevToken()
    {
        var prevSibling = GetPrevSibling();
        if (prevSibling is LuaSyntaxToken prevToken)
        {
            return prevToken;
        }

        return prevSibling?.LastToken();
    }

    public LuaSyntaxToken? LastToken()
    {
        var lastChild = ChildrenWithTokens.LastOrDefault();
        if (lastChild is LuaSyntaxToken token)
        {
            return token;
        }

        return lastChild?.LastToken();
    }

    // 从自身向前迭代, 直到找到一个类型为T的节点
    // public IEnumerable<T> PrevOfType<T>()
    //     where T : LuaSyntaxElement
    // {
    //     if (Parent?.ChildrenWithTokenArray is { } childrenWithTokenArray)
    //     {
    //         var selfPosition = ChildPosition;
    //         for (var i = selfPosition - 1; i >= 0; i--)
    //         {
    //             var nodeOrToken = childrenWithTokenArray[i];
    //             if (nodeOrToken is T node)
    //             {
    //                 yield return node;
    //             }
    //         }
    //     }
    // }

    public IEnumerable<T> NextOfType<T>()
        where T : LuaSyntaxElement
    {
        // if (Parent?.ChildrenWithTokenArray is { } childrenWithTokenArray)
        // {
        //     var selfPosition = ChildPosition;
        //     for (var i = selfPosition + 1; i < childrenWithTokenArray.Length; i++)
        //     {
        //         var nodeOrToken = childrenWithTokenArray[i];
        //         if (nodeOrToken is T node)
        //         {
        //             yield return node;
        //         }
        //     }
        // }
        var index = NextSiblingIndex;
        while (index != -1)
        {
            var element = Tree.GetElement(index)!;
            if (element is T node)
            {
                yield return node;
            }

            index = element.NextSiblingIndex;
        }
    }

    public LuaLocation Location => Tree.Document.GetLocation(Range);

    // 0 based line and col
    public LuaSyntaxToken? TokenAt(int line, int col)
    {
        var offset = Tree.Document.GetOffset(line, col);
        return TokenAt(offset);
    }

    public LuaSyntaxToken? TokenAt(int offset)
    {
        var node = this;
        while (node != null)
        {
            var nodeElement = node.ChildrenWithTokens.FirstOrDefault(it => it.Range.Contain(offset));
            if (nodeElement is LuaSyntaxToken token)
            {
                return token;
            }

            node = nodeElement;
        }

        return null;
    }

    public LuaSyntaxToken? TokenLeftBiasedAt(int line, int col)
    {
        if (col > 0)
        {
            col--;
        }

        var offset = Tree.Document.GetOffset(line, col);
        return TokenAt(offset);
    }

    public LuaSyntaxNode? NodeAt(int line, int col)
    {
        var token = TokenAt(line, col);
        return token?.Parent as LuaSyntaxNode;
    }

    public LuaSyntaxNode? NameNodeAt(int line, int col)
    {
        var token = TokenAt(line, col);
        if (token is null)
        {
            return null;
        }

        if (token is LuaNameToken or LuaNumberToken or LuaStringToken)
        {
            return token.Parent as LuaSyntaxNode;
        }

        token = TokenLeftBiasedAt(line, col);
        return token?.Parent as LuaSyntaxNode;
    }

    public LuaSyntaxNode? FindNode(SourceRange range, LuaSyntaxKind kind)
    {
        LuaSyntaxNode? node = this as LuaSyntaxNode;
        while (node != null)
        {
            if (node.Range.Equals(range) && node.Kind == kind)
            {
                return node;
            }

            node = node.ChildrenNode.FirstOrDefault(it => it.Range.Contain(range));
        }

        return null;
    }

    public void PushDiagnostic(DiagnosticSeverity severity, string message)
    {
        var diagnostic = new Diagnostic(severity, DiagnosticCode.SyntaxError, message, Range);
        Tree.PushDiagnostic(diagnostic);
    }

    public string UniqueId => $"{DocumentId}_{Range.StartOffset}_{Range.Length}_{RawKind}";

    public int Position => Range.StartOffset;

    public override int GetHashCode()
    {
        return HashCode.Combine(DocumentId, Range, RawKind);
    }

    public bool Equals(LuaSyntaxElement? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return DocumentId == other.DocumentId
               && Range.Equals(other.Range)
               && RawKind == other.RawKind;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaSyntaxElement);
    }
}
