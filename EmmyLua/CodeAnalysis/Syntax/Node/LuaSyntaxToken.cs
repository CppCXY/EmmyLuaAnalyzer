using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken : LuaSyntaxElement
{
    public LuaSyntaxToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
        _lazyInit = true;
    }

    public LuaTokenKind Kind => Green.TokenKind;

    public ReadOnlySpan<char> Text => Tree.Source.Text.AsSpan(Green.Range.StartOffset, Green.Range.Length);

    public string RepresentText => Text.ToString();
}
