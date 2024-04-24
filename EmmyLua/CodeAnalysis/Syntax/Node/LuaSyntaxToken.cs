using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxElement(greenNode, tree, parent, startOffset)
{
    public LuaTokenKind Kind => (LuaTokenKind)RawKind;

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);

    public string RepresentText => Text.ToString();

    protected override IEnumerable<LuaSyntaxElement> ChildrenElements => Enumerable.Empty<LuaSyntaxElement>();

    public override void AddChild(LuaSyntaxElement child)
    {
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsAndSelf
    {
        get { yield return this; }
    }

    public override IEnumerable<LuaSyntaxElement> Descendants => Enumerable.Empty<LuaSyntaxElement>();

    public override IEnumerable<LuaSyntaxElement> DescendantsInRange(SourceRange range)
    {
        if (range.Intersect(Range))
        {
            yield return this;
        }
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsWithToken => Enumerable.Empty<LuaSyntaxElement>();

    public override IEnumerable<LuaSyntaxElement> DescendantsAndSelfWithTokens
    {
        get { yield return this; }
    }
}
