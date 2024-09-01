using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Visibility(SearchContext context)
{
    private Dictionary<SyntaxElementId, LuaType> Cache { get; } = new();

    public bool CheckVisible(LuaSyntaxElement element, LuaSymbol symbol)
    {
        if (symbol.IsPublic)
        {
            return true;
        }

        if (symbol.IsPackage && symbol.DocumentId == element.DocumentId)
        {
            return true;
        }

        if (element is LuaIndexExprSyntax indexExpr)
        {
            return CheckIndexVisible(indexExpr, symbol);
        }

        return true;
    }

    private bool CheckIndexVisible(LuaIndexExprSyntax indexExpr, LuaSymbol symbol)
    {
        var luaFuncStats = indexExpr.Ancestors.OfType<LuaFuncStatSyntax>().ToList();
        if (luaFuncStats.Count == 0)
        {
            return false;
        }

        var prefixType = context.Infer(indexExpr.PrefixExpr);
        if (prefixType is LuaUnionType unionType)
        {
            prefixType = unionType.Remove(Builtin.Nil, context);
        }

        foreach (var luaFuncStat in luaFuncStats)
        {
            var envType = GetFuncEnvType(luaFuncStat);
            if (envType is null || envType.IsSameType(Builtin.Unknown, context))
            {
                continue;
            }

            if (symbol.IsPrivate && envType.IsSameType(prefixType, context))
            {
                return true;
            }
            else if (symbol.IsProtected && envType.IsSubTypeOf(prefixType, context))
            {
                return true;
            }
        }

        return false;
    }

    private LuaType? GetFuncEnvType(LuaFuncStatSyntax funcStat)
    {
        if (Cache.TryGetValue(funcStat.UniqueId, out var type))
        {
            return type;
        }

        if (funcStat.IndexExpr is { PrefixExpr: { } prefixExpr })
        {
            var prefixType = context.Infer(prefixExpr);
            Cache[funcStat.UniqueId] = prefixType;
            return prefixType;
        }

        return null;
    }
}
