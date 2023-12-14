using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxElement(green, tree, parent)
{
    public LuaSyntaxKind Kind => Green.SyntaxKind;
}
