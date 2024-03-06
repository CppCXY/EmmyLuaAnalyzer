using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class CallExprInfer
{
    public static LuaType InferCallExpr(LuaCallExprSyntax callExpr, SearchContext context)
    {
        LuaType ret = Builtin.Unknown;
        var prefixExpr = callExpr.PrefixExpr;
        var accessPath = callExpr.AccessPath;
        if (context.Compilation.Workspace.Features.RequireLikeFunction.Contains(accessPath))
        {
            return InferRequire(callExpr, context);
        }

        var luaType = context.Infer(prefixExpr);
        TypeHelper.Each(luaType, type =>
        {
            switch (type)
            {
                case LuaMethodType luaMethod:
                {
                    // var perfectSig = luaMethod.FindPerfectSignature(callExpr, context);
                    // if (perfectSig.ReturnTypes is { } retTy)
                    // {
                    //     ret = LuaUnion.UnionType(ret, retTy);
                    // }

                    break;
                }
            }
        });

        // TODO class.new return self
        // if (prefixExpr is LuaIndexExprSyntax indexExpr)
        // {
        //     var fnName = indexExpr.Name?.RepresentText;
        //     if (fnName is not null)
        //     {
        //         var fnSymbol = context.Compilation.GetSymbol(fnName);
        //     }
        // }

        // return TryUnwrapReturn(callExpr, context, ret);
        throw new NotImplementedException();
    }

    // private static LuaType TryUnwrapReturn(LuaCallExprSyntax callExprSyntax, SearchContext context, ILuaType ret)
    // {
    //     while (true)
    //     {
    //         switch (ret)
    //         {
    //             case LuaTypeRef refTy:
    //             {
    //                 return refTy.GetType(context);
    //             }
    //             case LuaMultiRetType multiRetType:
    //             {
    //                 if (callExprSyntax.Parent is LuaTableFieldSyntax field)
    //                 {
    //                     var table = field.ParentTable;
    //                     if (ReferenceEquals(table?.FieldList.LastOrDefault(), field))
    //                     {
    //                         return multiRetType;
    //                     }
    //                 }
    //                 else if (callExprSyntax.Parent is LuaLocalStatSyntax localStat)
    //                 {
    //                     if (ReferenceEquals(localStat.ExprList.LastOrDefault(), callExprSyntax))
    //                     {
    //                         return multiRetType;
    //                     }
    //                 }
    //                 else if (callExprSyntax.Parent is LuaAssignStatSyntax assignStat)
    //                 {
    //                     if (ReferenceEquals(assignStat.ExprList.LastOrDefault(), callExprSyntax))
    //                     {
    //                         return multiRetType;
    //                     }
    //                 }
    //
    //                 ret = multiRetType.GetRetType(0) ?? context.Compilation.Builtin.Unknown;
    //                 continue;
    //             }
    //             default:
    //             {
    //                 return ret;
    //             }
    //         }
    //     }
    // }

    private static LuaType InferRequire(LuaCallExprSyntax callExpr, SearchContext context)
    {
        var firstArg = callExpr.ArgList?.ArgList.FirstOrDefault();
        if (firstArg is LuaLiteralExprSyntax { Literal: LuaStringToken { Value: { } modulePath } })
        {
            if (context.Compilation.Workspace.Features.VirtualModule.TryGetValue(modulePath, out var realModule))
            {
                modulePath = realModule;
            }

            var document = context.Compilation.Workspace.ModuleGraph.FindModule(modulePath);
            if (document is not null)
            {
                return context.Infer(document.SyntaxTree.SyntaxRoot);
            }
        }

        return Builtin.Unknown;
    }
}
