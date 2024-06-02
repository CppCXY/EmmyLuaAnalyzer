using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken(int index, LuaSyntaxTree tree)
    : LuaSyntaxElement(index, tree)
{
    public LuaTokenKind Kind => (LuaTokenKind)RawKind;

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);

    public string RepresentText => Text.ToString();

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
