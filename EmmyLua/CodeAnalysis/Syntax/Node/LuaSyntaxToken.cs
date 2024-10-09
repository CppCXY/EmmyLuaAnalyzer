using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken(int index, LuaSyntaxTree tree): LuaSyntaxElement(index, tree)
{
    public LuaTokenKind Kind => Tree.GetTokenKind(ElementId);

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);

    public string RepresentText => Text.ToString();
}
