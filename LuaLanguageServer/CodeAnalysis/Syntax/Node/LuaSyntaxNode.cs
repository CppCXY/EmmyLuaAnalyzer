using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode : LuaSyntaxElement
{
    public LuaSyntaxNode(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(green, tree, parent)
    {
    }

    public LuaSyntaxKind Kind => Green.SyntaxKind;
}

public class LuaSyntaxDocNode : LuaSyntaxNode
{
    public LuaSyntaxDocNode(GreenNode green, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(green, tree, parent)
    {
    }

}
