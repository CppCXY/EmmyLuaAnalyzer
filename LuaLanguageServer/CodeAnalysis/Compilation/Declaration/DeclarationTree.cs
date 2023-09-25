using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationTree
{
    public LuaSyntaxTree LuaSyntaxTree { get; }

    public static DeclarationTree From(LuaSyntaxTree tree)
    {
        return new DeclarationTree(tree);
    }

    private DeclarationTree(LuaSyntaxTree tree)
    {
        LuaSyntaxTree = tree;
    }


}
