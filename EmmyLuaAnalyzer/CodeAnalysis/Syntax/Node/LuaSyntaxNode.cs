using EmmyLuaAnalyzer.CodeAnalysis.Kind;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Green;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxElement(green, tree, parent)
{
    public LuaSyntaxKind Kind => Green.SyntaxKind;
}
