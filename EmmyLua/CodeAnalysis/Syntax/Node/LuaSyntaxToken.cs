using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxElement(greenNode, tree, parent, startOffset)
{
    public LuaTokenKind Kind => (LuaTokenKind)RawKind;

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);

    public string RepresentText => Text.ToString();

    protected override IEnumerable<LuaSyntaxElement> ChildrenElements => [];

    public override void AddChild(LuaSyntaxElement child)
    {
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsAndSelf
    {
        get { yield return this; }
    }

    public override IEnumerable<LuaSyntaxElement> Descendants => [];

    public override IEnumerable<LuaSyntaxElement> DescendantsInRange(SourceRange range)
    {
        if (range.Intersect(Range))
        {
            yield return this;
        }
    }

    public override IEnumerable<LuaSyntaxElement> DescendantsWithToken => [];

    public override IEnumerable<LuaSyntaxElement> DescendantsAndSelfWithTokens
    {
        get { yield return this; }
    }
}
