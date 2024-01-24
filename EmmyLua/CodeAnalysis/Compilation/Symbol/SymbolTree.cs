using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class SymbolTree(LuaSyntaxTree tree, IReadOnlyDictionary<LuaSyntaxElement, SymbolScope> scopeOwners)
{
    public LuaSyntaxTree LuaSyntaxTree { get; } = tree;

    public DocumentId Id => LuaSyntaxTree.Document.Id;

    public SymbolScope? RootScope { get; internal set; }

    public int GetPosition(LuaSyntaxElement element) => element.Range.StartOffset;

    public Symbol? FindDeclaration(LuaSyntaxElement element)
    {
        switch (element)
        {
            case LuaNameExprSyntax nameExpr:
            {
                var scope = FindScope(nameExpr);
                return scope?.FindNameExpr(nameExpr);
            }
            case LuaParamDefSyntax paramDef:
            {
                var scope = FindScope(paramDef);
                return scope?.FindParamDef(paramDef);
            }
            case LuaLocalNameSyntax localName:
            {
                var scope = FindScope(localName);
                return scope?.FindLocalName(localName);
            }
        }

        return null;
    }

    public SymbolScope? FindScope(LuaSyntaxElement element)
    {
        var cur = element;
        while (cur != null)
        {
            if (scopeOwners.TryGetValue(cur, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }

}
