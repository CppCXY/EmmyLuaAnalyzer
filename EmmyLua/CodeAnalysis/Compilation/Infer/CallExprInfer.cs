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

        var luaType = context.Infer(prefixExpr);
        var args = callExpr.ArgList?.ArgList.ToList() ?? [];
        TypeHelper.Each(luaType, type =>
        {
            switch (type)
            {
                case LuaMethodType luaMethod:
                {
                    var perfectSig = luaMethod.FindPerfectMatchSignature(callExpr, args, context);
                    if (perfectSig.ReturnType is { } retTy)
                    {
                        returnType = returnType.Union(retTy);
                    }

                    break;
                }
            }
        });


        if (returnType.Equals(Builtin.Unknown) && prefixExpr is LuaIndexExprSyntax indexExpr)
        {
            var fnName = indexExpr.Name;
            if (fnName is not null && string.Equals(fnName, "new", StringComparison.CurrentCultureIgnoreCase))
            {
                return context.Infer(indexExpr.PrefixExpr);
            }
        }

        return UnwrapReturn(callExpr, context, returnType, true);
    }

    /// <summary>
    /// 主要用于把ReturnMultiType根据调用情况取消掉wrapper
    /// </summary>
    /// <param name="callExprSyntax"></param>
    /// <param name="context"></param>
    /// <param name="ret"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    private static LuaType UnwrapReturn(
        LuaCallExprSyntax callExprSyntax,
        SearchContext context,
        LuaType ret,
        bool top = false)
    {
        if (top && ret is LuaUnionType unionType)
        {
            var types = unionType.UnionTypes.Select(t => UnwrapReturn(callExprSyntax, context, t)).ToList();
            return new LuaUnionType(types);
        }

        if (ret is LuaMultiReturnType multiRetType)
        {
            if (callExprSyntax.Parent is LuaTableFieldSyntax field)
            {
                var table = field.ParentTable;
                if (ReferenceEquals(table?.FieldList.LastOrDefault(), field))
                {
                    return multiRetType;
                }
            }
            else if (callExprSyntax.Parent is LuaLocalStatSyntax localStat)
            {
                if (ReferenceEquals(localStat.ExprList.LastOrDefault(), callExprSyntax))
                {
                    return multiRetType;
                }
            }
            else if (callExprSyntax.Parent is LuaAssignStatSyntax assignStat)
            {
                if (ReferenceEquals(assignStat.ExprList.LastOrDefault(), callExprSyntax))
                {
                    return multiRetType;
                }
            }
            else if (callExprSyntax.Parent is LuaCallExprSyntax callExpr2)
            {
                if (ReferenceEquals(callExpr2.ArgList?.ArgList.LastOrDefault(), callExprSyntax))
                {
                    return multiRetType;
                }
            }

            ret = multiRetType.RetTypes.FirstOrDefault() ?? Builtin.Unknown;
        }

        return ret;
    }

    private static LuaType InferRequire(LuaCallExprSyntax callExpr, SearchContext context)
    {
        var firstArg = callExpr.ArgList?.ArgList.FirstOrDefault();
        if (firstArg is LuaLiteralExprSyntax { Literal: LuaStringToken { Value: { } modulePath } })
        {
            var document = context.Compilation.Workspace.ModuleGraph.FindModule(modulePath);
            if (document is not null)
            {
                return context.Infer(document.SyntaxTree.SyntaxRoot);
            }
        }

        return Builtin.Unknown;
    }
}
