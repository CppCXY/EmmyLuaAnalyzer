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
}
