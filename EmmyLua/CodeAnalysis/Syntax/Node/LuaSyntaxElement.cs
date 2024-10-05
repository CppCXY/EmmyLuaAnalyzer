using System.Collections;
using System.Text;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Visitor;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public abstract class LuaSyntaxElement(int index, LuaSyntaxTree tree)
    : IEquatable<LuaSyntaxElement>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public int ElementId { get; } = index;

    public LuaSyntaxTree Tree { get; } = tree;

    protected int RawKind => Tree.GetRawKind(ElementId);

    protected int ParentIndex => Tree.GetParent(ElementId);

    protected int ChildStartIndex => Tree.GetChildStart(ElementId);

    protected int ChildFinishIndex => Tree.GetChildEnd(ElementId);

    public LuaDocumentId DocumentId => Tree.Document.Id;

    public SourceRange Range => Tree.GetSourceRange(ElementId);

    public LuaSyntaxNode? Parent => Tree.GetElement(ParentIndex) as LuaSyntaxNode;

    public SyntaxElementId UniqueId => new(DocumentId, ElementId);

    public string UniqueString => UniqueId.ToString();

    public int Position => Range.StartOffset;

    protected IEnumerable<LuaSyntaxElement> ChildrenElements
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
                var element = Tree.GetElement(i);
                if (element is not null)
                {
                    yield return element;
                }
            }
        }
    }

    public IEnumerable<LuaSyntaxNode> ChildrenNode
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
                if (Tree.IsNode(i))
                {
                    var element = Tree.GetElement(i);
                    if (element is not null)
                    {
                        yield return (element as LuaSyntaxNode)!;
                    }
                }
            }
        }
    }

    // for better performance
    public IEnumerable<LuaSyntaxNode> ChildrenNodeFor(HashSet<LuaSyntaxKind> kinds)
    {
        var start = ChildStartIndex;
        if (start == -1)
        {
            yield break;
        }

        var finish = ChildFinishIndex;
        for (var i = start; i <= finish; i++)
        {
            if (kinds.Contains(Tree.GetSyntaxKind(i)))
            {
                var element = Tree.GetElement(i);
                if (element is LuaSyntaxNode node)
                {
                    yield return node;
                }
            }
        }
    }

    public IEnumerable<LuaSyntaxToken> ChildrenTokenFor(HashSet<LuaTokenKind> kinds)
    {
        var start = ChildStartIndex;
        if (start == -1)
        {
            yield break;
        }

        var finish = ChildFinishIndex;
        for (var i = start; i <= finish; i++)
        {
            if (kinds.Contains(Tree.GetTokenKind(i)))
            {
                var element = Tree.GetElement(i);
                if (element is LuaSyntaxToken token)
                {
                    yield return token;
                }
            }
        }
    }

    public IEnumerable<LuaSyntaxElement> ChildrenWithTokens => ChildrenElements;

    // 遍历所有后代, 包括自己
    public abstract IEnumerable<LuaSyntaxElement> DescendantsAndSelf { get; }

    // 不包括自己
    public abstract IEnumerable<LuaSyntaxElement> Descendants { get; }

    public abstract IEnumerable<LuaSyntaxElement> DescendantsInRange(SourceRange range);

    public abstract IEnumerable<LuaSyntaxElement> DescendantsWithToken { get; }

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
    public abstract IEnumerable<LuaSyntaxElement> DescendantsAndSelfWithTokens { get; }

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
        return ChildrenElements.OfType<T>().FirstOrDefault();
    }

    public LuaSyntaxToken? FirstChildToken(LuaTokenKind kind)
    {
        return FirstChildToken(it => it == kind);
    }

    public LuaSyntaxToken? FirstChildToken()
    {
        return FirstChildToken(it => it != LuaTokenKind.None);
    }

    public LuaSyntaxToken? FirstChildToken(Func<LuaTokenKind, bool> predicate)
    {
        return ChildrenElements.OfType<LuaSyntaxToken>().FirstOrDefault(it => predicate(it.Kind));
    }

    public IEnumerable<T> ChildrenElement<T>() where T : LuaSyntaxElement => ChildrenElements.OfType<T>();

    public IEnumerable<T> ChildNodesBeforeToken<T>(LuaTokenKind kind) where T : LuaSyntaxElement
    {
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
        var start = ChildStartIndex;
        if (start == -1)
        {
            yield break;
        }

        var finish = ChildFinishIndex;
        for (var i = start; i <= finish; i++)
        {
            if (Tree.GetTokenKind(i) == kind)
            {
                var element = Tree.GetElement(i);
                if (element is not null)
                {
                    yield return (element as LuaSyntaxToken)!;
                }
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
        var parent = Parent;
        if (parent is null)
        {
            return null;
        }

        var start = parent.ChildStartIndex;
        if (start == -1)
        {
            return null;
        }

        var finish = parent.ChildFinishIndex;
        var nextElementId = ElementId + next;
        return nextElementId <= finish ? Tree.GetElement(nextElementId) : null;
    }

    public LuaSyntaxElement? GetPrevSibling(int prev = 1)
    {
        var parent = Parent;
        if (parent is null)
        {
            return null;
        }

        var start = parent.ChildStartIndex;
        if (start == -1)
        {
            return null;
        }

        var prevElementId = ElementId - prev;
        return prevElementId >= start ? Tree.GetElement(prevElementId) : null;
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
    public IEnumerable<T> PrevOfType<T>()
        where T : LuaSyntaxElement
    {
        var parent = Parent;
        if (parent is null)
        {
            yield break;
        }

        var start = parent.ChildStartIndex;
        if (start == -1)
        {
            yield break;
        }

        for (var i = ElementId - 1; i >= start; i--)
        {
            var element = Tree.GetElement(i);
            if (element is T node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<T> NextOfType<T>()
        where T : LuaSyntaxElement
    {
        var parent = Parent;
        if (parent is null)
        {
            yield break;
        }

        var finish = parent.ChildFinishIndex;
        if (finish == -1)
        {
            yield break;
        }

        for (var i = ElementId + 1; i <= finish; i++)
        {
            var element = Tree.GetElement(i);
            if (element is T node)
            {
                yield return node;
            }
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
        if (offset == Tree.Document.Text.Length)
        {
            offset--;
        }

        return offset < 0 ? null : TokenAt(offset);
    }

    public LuaSyntaxNode? NodeAt(int line, int col)
    {
        var token = TokenAt(line, col);
        return token?.Parent;
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
            return token.Parent;
        }

        token = TokenLeftBiasedAt(line, col);
        return token?.Parent;
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

    public void VisitSyntax(LuaSyntaxVisitor visitor) => visitor.Visit(this);

    public void PushDiagnostic(DiagnosticSeverity severity, string message)
    {
        var diagnostic = new Diagnostic(severity, DiagnosticCode.SyntaxError, message, Range);
        Tree.PushDiagnostic(diagnostic);
    }

    public LuaElementPtr<TNode> ToPtr<TNode>()
        where TNode : LuaSyntaxElement
        => new(UniqueId);

    public override int GetHashCode()
    {
        return UniqueId.GetHashCode();
    }

    public bool Equals(LuaSyntaxElement? other)
    {
        if (other is null)
        {
            return false;
        }

        return UniqueId == other.UniqueId;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaSyntaxElement);
    }
}
