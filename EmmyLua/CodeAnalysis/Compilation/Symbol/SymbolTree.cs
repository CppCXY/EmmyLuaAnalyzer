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

    public Symbol? FindSymbol(LuaSyntaxElement element)
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

    public Declaration? FindDeclaration(LuaExprSyntax expr, SearchContext context)
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

    private Declaration? FindNameDeclaration(LuaNameExprSyntax nameExpr, SearchContext context)
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

    private Declaration? FindIndexDeclaration(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        if (indexExpr.PrefixExpr is { } prefixExpr)
        {
            var prefixType = context.Infer(prefixExpr);
            switch (indexExpr)
            {
                case { DotOrColonIndexName: { } nameToken }:
                    return context.FindMember(prefixType, nameToken.RepresentText).FirstOrDefault();
                case { IndexKeyExpr: LuaLiteralExprSyntax literal }:
                    return literal.Literal switch
                    {
                        LuaStringToken stringToken => context.FindMember(prefixType, stringToken.Value).FirstOrDefault(),
                        LuaIntegerToken luaIntegerToken => context.FindMember(prefixType, $"[{luaIntegerToken.Value}]")
                            .FirstOrDefault(),
                        _ => context.FindMember(prefixType, literal.Literal.RepresentText).FirstOrDefault()
                    };
                case { IndexKeyExpr: { } expr }:
                {
                    // var indexType = context.Infer(expr);
                    // return prefixType.IndexMember(indexType, context).FirstOrDefault();
                    throw new NotImplementedException();
                }
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

    public IEnumerable<Symbol> Symbols => RootScope?.Descendants ?? Enumerable.Empty<Symbol>();
}
