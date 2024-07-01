using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class CallExprInfer
{
    public static LuaType InferCallExpr(LuaCallExprSyntax callExpr, SearchContext context)
    {
        LuaType returnType = Builtin.Unknown;
        var prefixExpr = callExpr.PrefixExpr;
        var callName = callExpr.Name;
        if (context.Compilation.Workspace.Features.RequireLikeFunction.Contains(callName))
        {
            return InferRequire(callExpr, context);
        }

        var luaType = context.InferAndUnwrap(prefixExpr);
        var args = callExpr.ArgList?.ArgList.ToList() ?? [];
        context.FindMethodsForType(luaType, luaMethod =>
        {
            var perfectSig = MethodInfer.FindPerfectMatchSignature(luaMethod, callExpr, args, context);
            if (perfectSig.ReturnType is { } retTy)
            {
                // ReSharper disable once AccessToModifiedClosure
                returnType = returnType.Union(retTy);
            }
        });

        // TODO: use config enable this feature
        // if (returnType.Equals(Builtin.Unknown) && prefixExpr is LuaIndexExprSyntax indexExpr)
        // {
        //     var fnName = indexExpr.Name;
        //     if (fnName is not null && string.Equals(fnName, "new", StringComparison.CurrentCultureIgnoreCase))
        //     {
        //         returnType = context.Infer(indexExpr.PrefixExpr);
        //     }
        // }

        return UnwrapReturn(callExpr, context, returnType, 0);
    }

    /// <summary>
    /// 主要用于把ReturnMultiType根据调用情况取消掉wrapper
    /// </summary>
    /// <param name="callExprSyntax"></param>
    /// <param name="context"></param>
    /// <param name="ret"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private static LuaType UnwrapReturn(
        LuaCallExprSyntax callExprSyntax,
        SearchContext context,
        LuaType ret,
        int level = 0)
    {
        switch (ret)
        {
            case LuaUnionType unionType:
            {
                return UnwrapUnion(unionType, callExprSyntax, context, level);
            }
            case LuaMultiReturnType multiRetType:
            {
                return UnwrapMultiReturn(multiRetType, callExprSyntax, context, level);
            }
            case LuaVariadicType variadicType:
            {
                return UnwrapVariadicType(variadicType, callExprSyntax, context, level);
            }
            case LuaNamedType namedType:
            {
                if (namedType.Name == "self")
                {
                    if (callExprSyntax.PrefixExpr is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr })
                    {
                        return context.Infer(prefixExpr);
                    }
                }

                break;
            }
        }

        return ret;
    }

    private static LuaType UnwrapUnion(LuaUnionType unionType, LuaCallExprSyntax callExprSyntax, SearchContext context,
        int level)
    {
        if (level > 0)
        {
            return unionType;
        }

        var types = unionType.UnionTypes.Select(t => UnwrapReturn(callExprSyntax, context, t, level)).ToList();
        return new LuaUnionType(types);
    }

    private static LuaType UnwrapMultiReturn(LuaMultiReturnType multiReturnType, LuaCallExprSyntax callExprSyntax,
        SearchContext context, int level)
    {
        if (level > 0)
        {
            return multiReturnType;
        }

        var retType = IsLastCallExpr(callExprSyntax) ? multiReturnType : multiReturnType.GetElementType(0);

        return UnwrapReturn(callExprSyntax, context, retType, level + 1);
    }

    private static LuaType UnwrapVariadicType(LuaVariadicType variadicType, LuaCallExprSyntax callExprSyntax,
        SearchContext context, int level)
    {
        if (level > 1)
        {
            return variadicType;
        }

        if (!IsLastCallExpr(callExprSyntax))
        {
            return new LuaMultiReturnType(variadicType.BaseType);
        }

        return variadicType.BaseType;
    }

    private static bool IsLastCallExpr(LuaCallExprSyntax callExpr)
    {
        if (callExpr.Parent is LuaTableFieldSyntax field)
        {
            var table = field.ParentTable;
            return field.Equals(table?.FieldList.LastOrDefault());
        }

        if (callExpr.Parent is LuaLocalStatSyntax localStat)
        {
            return callExpr.Equals(localStat.ExprList.LastOrDefault());
        }

        if (callExpr.Parent is LuaAssignStatSyntax assignStat)
        {
            return callExpr.Equals(assignStat.ExprList.LastOrDefault());
        }

        if (callExpr.Parent is LuaCallExprSyntax callExpr2)
        {
            return callExpr.Equals(callExpr2.ArgList?.ArgList.LastOrDefault());
        }

        return false;
    }

    private static LuaType InferRequire(LuaCallExprSyntax callExpr, SearchContext context)
    {
        var firstArg = callExpr.ArgList?.ArgList.FirstOrDefault();
        if (firstArg is LuaLiteralExprSyntax { Literal: LuaStringToken { Value: { } modulePath } })
        {
            var document = context.Compilation.Workspace.ModuleManager.FindModule(modulePath);
            if (document is not null)
            {
                return context.Infer(document.SyntaxTree.SyntaxRoot);
            }
        }

        return Builtin.Unknown;
    }
}
