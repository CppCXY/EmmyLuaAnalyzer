
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
                    var args = callExpr.ArgList?.ArgList;
                    if (args == null) return;
                    var perfectSig = luaMethod.FindPerfectSignature(args, context);
                    if (perfectSig.ReturnType is { } retTy)
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

        return ret;
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
