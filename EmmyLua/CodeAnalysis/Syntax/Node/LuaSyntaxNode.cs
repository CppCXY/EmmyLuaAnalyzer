using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxElement(green, tree, parent)
{
    public LuaSyntaxKind Kind => Green.SyntaxKind;
}
