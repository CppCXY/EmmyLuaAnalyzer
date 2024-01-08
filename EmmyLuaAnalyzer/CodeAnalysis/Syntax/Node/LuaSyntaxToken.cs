using EmmyLuaAnalyzer.CodeAnalysis.Kind;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Green;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;

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
