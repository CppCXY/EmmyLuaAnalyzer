using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class SymbolTree(LuaSyntaxTree tree, IReadOnlyDictionary<LuaSyntaxElement, SymbolScope> scopeOwners)
{
    public LuaSyntaxTree SyntaxTree { get; } = tree;

    public SymbolScope? RootScope { get; internal set; }

    public LuaSymbol? FindSymbol(LuaSyntaxElement element)
    {
        switch (element)
        {
            case LuaNameExprSyntax or LuaParamDefSyntax or LuaLocalNameSyntax or LuaIndexExprSyntax:
            {
                var scope = FindScope(element);
                return scope?.FindSymbol(element);
            }
        }

        return null;
    }

    public LuaDeclaration? FindDeclaration(LuaExprSyntax expr, SearchContext context)
    {
        switch (expr)
        {
            case LuaNameExprSyntax nameExpr:
            {
                return FindNameDeclaration(nameExpr, context);
            }
            case LuaIndexExprSyntax indexExpr:
            {
                return FindIndexDeclaration(indexExpr, context);
            }
        }

        return null;
    }

    private LuaDeclaration? FindNameDeclaration(LuaNameExprSyntax nameExpr, SearchContext context)
    {
        if (nameExpr.Name is { } name)
        {
            var scope = FindScope(nameExpr);
            var declaration = scope?.FindNameDeclaration(nameExpr);
            if (declaration is not null)
            {
                return declaration;
            }
            return context.Compilation.ProjectIndex.GetGlobal(name.RepresentText).FirstOrDefault();
        }

        return null;
    }

    private LuaDeclaration? FindIndexDeclaration(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        if (indexExpr.PrefixExpr is { } prefixExpr)
        {
            var prefixType = context.Infer(prefixExpr);
            var name = indexExpr.Name;
            if (name is not null)
            {
                return context.FindMember(prefixType, name).FirstOrDefault();
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

    public IEnumerable<LuaSymbol> Symbols => RootScope?.Descendants ?? Enumerable.Empty<LuaSymbol>();
}
