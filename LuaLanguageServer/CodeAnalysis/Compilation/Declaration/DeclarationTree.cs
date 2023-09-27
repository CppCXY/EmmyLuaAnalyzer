using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationTree
{
    public LuaSyntaxTree LuaSyntaxTree { get; }

    private Stack<DeclarationScope> _scopes = new();

    public static DeclarationTree From(LuaSyntaxTree tree)
    {
        return new DeclarationTree(tree);
    }

    private DeclarationTree(LuaSyntaxTree tree)
    {
        LuaSyntaxTree = tree;
    }

    public int GetPosition(LuaSyntaxElement element) => element.Green.Range.StartOffset;

    private void Build()
    {
        _scopes.Clear();
        foreach (var element in LuaSyntaxTree.SyntaxRoot.DescendantsWithToken)
        {

        }



    }

    private DeclarationScope Push(LuaSyntaxElement element)
    {
        // var position = GetPosition(element);
        // switch (element)
        // {
        //     case LuaLocalStatSyntax localStatSyntax:
        //     {
        //         return Push(new DeclarationScope(this, position, null), element);
        //     }
        //     case LuaRepeatStatSyntax repeatStatSyntax:
        //     {
        //
        //     }
        //     case LuaForRangeStatSyntax forRangeStatSyntax:
        //     {
        //
        //     }
        // }
        //
        // return Push();
        throw new NotImplementedException();
    }

    private DeclarationScope Push(DeclarationScope scope, LuaSyntaxElement element)
    {
        _scopes.Push(scope);
        return scope;
    }
}
