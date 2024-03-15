using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public class LuaDeclarationTree(LuaSyntaxTree tree, IReadOnlyDictionary<LuaSyntaxElement, DeclarationScope> scopeOwners)
{
    public LuaSyntaxTree SyntaxTree { get; } = tree;

    public DeclarationScope? RootScope { get; internal set; }

    public LuaDeclaration? FindDeclaration(LuaSyntaxElement element, SearchContext context)
    {
        switch (element)
        {
            case LuaNameExprSyntax nameExpr:
            {
                return FindNameDeclaration(nameExpr, context);
            }
            case LuaIndexExprSyntax indexExpr:
            {
                return FindIndexDeclaration(indexExpr, context);
            }
            case LuaNameExprSyntax or LuaParamDefSyntax or LuaLocalNameSyntax or LuaIndexExprSyntax:
            {
                var scope = FindScope(element);
                return scope?.FindDeclaration(element);
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

    public DeclarationScope? FindScope(LuaSyntaxElement element)
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
