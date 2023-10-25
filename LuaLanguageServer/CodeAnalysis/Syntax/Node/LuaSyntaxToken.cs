using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

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
