using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocType : LuaSyntaxNode
{
    public LuaDocType(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocLiteralType : LuaDocType
{
    public LuaDocLiteralType(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}
