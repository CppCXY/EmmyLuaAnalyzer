using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationTree
{
    public LuaSyntaxTree LuaSyntaxTree { get; }

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners;

    public DeclarationTree(LuaSyntaxTree tree, Dictionary<LuaSyntaxElement, DeclarationScope> scopeOwners)
    {
        LuaSyntaxTree = tree;
        _scopeOwners = scopeOwners;
    }

    public int GetPosition(LuaSyntaxElement element) => element.Green.Range.StartOffset;

    public Declaration? FindDeclaration(LuaNameExprSyntax nameExpr)
    {
        var scope = FindScope(nameExpr);
        return scope?.FindNameExpr(nameExpr)?.FirstDeclaration;
    }

    // public bool IsGlobalName(LuaNameExprSyntax nameExpr)
    // {
    //     var scope = FindScope(nameExpr);
    //     return scope?.FindNameExpr(nameExpr)?.IsGlobal ?? false;
    // }

    public DeclarationScope? FindScope(LuaSyntaxElement element)
    {
        var cur = element;
        while (cur != null)
        {
            if (_scopeOwners.TryGetValue(cur, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }

    public void WalkUp(LuaSyntaxElement element, Func<Declaration, bool> process)
    {
        var scope = FindScope(element);
        scope?.WalkUp(GetPosition(element), 0, process);
    }

    public void WalkUpLocal(LuaSyntaxElement element, Func<Declaration, bool> process)
    {
        WalkUp(element, declaration =>
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (declaration.IsLocal)
            {
                return process(declaration);
            }

            return true;
        });
    }

    private static bool IsScopeOwner(LuaSyntaxNode node)
        => node is LuaBlockSyntax or LuaFuncStatSyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax
            or LuaForStatSyntax;
}
