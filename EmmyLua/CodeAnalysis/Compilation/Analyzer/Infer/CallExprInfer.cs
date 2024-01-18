using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

public class CallExprInfer
{
    public Dictionary<string, Func<LuaCallExprSyntax, SearchContext, ILuaType>> CallExprHandles = new();

    public CallExprInfer()
    {
        CallExprHandles.Add("require", InferRequire);
        // TODO
        // CallExprHandles.Add("pcall", InferPcall);
        // CallExprHandles.Add("type", InferType);
    }

    public ILuaType InferCallExpr(LuaCallExprSyntax callExpr, SearchContext context)
    {
        ILuaType ret = context.Compilation.Builtin.Unknown;
        var prefixExpr = callExpr.PrefixExpr;
        var accessPath = callExpr.AccessPath;
        if (CallExprHandles.TryGetValue(accessPath, out var handle))
        {
            return handle(callExpr, context);
        }

        var luaType = context.Infer(prefixExpr);
        LuaUnion.Each(luaType, type =>
        {
            switch (type)
            {
                case LuaMethod luaMethod:
                {
                    var perfectSig = luaMethod.FindPerfectSignature(callExpr, context);
                    if (perfectSig.ReturnTypes is { } retTy)
                    {
                        ret = LuaUnion.UnionType(ret, retTy);
                    }

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

        return TryUnwrapReturn(callExpr, context, ret);
    }

    private static ILuaType TryUnwrapReturn(LuaCallExprSyntax callExprSyntax, SearchContext context, ILuaType ret)
    {
        while (true)
        {
            switch (ret)
            {
                case LuaTypeRef refTy:
                {
                    return refTy.GetType(context);
                }
                case LuaMultiRetType multiRetType:
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

                    ret = multiRetType.GetRetType(0) ?? context.Compilation.Builtin.Unknown;
                    continue;
                }
                default:
                {
                    return ret;
                }
            }
        }
    }

    private static ILuaType InferRequire(LuaCallExprSyntax callExpr, SearchContext context)
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

        return context.Compilation.Builtin.Unknown;
    }
}
