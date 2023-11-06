using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public class CallExprInfer
{
    public Dictionary<string, Func<LuaCallExprSyntax, SearchContext, ILuaType>> CallExprHandles = new();

    public CallExprInfer()
    {
        CallExprHandles.Add("require", InferRequire);
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

        var symbol = context.Infer(prefixExpr);
        Union.Each(symbol, s =>
        {
            switch (s)
            {
                case Func func:
                {
                    var args = callExpr.ArgList?.ArgList;
                    if (args == null) return;
                    var argSymbols = args.Select(context.Infer);
                    var perfectSig = func.FindPerfectSignature(argSymbols, context);
                    if (perfectSig.ReturnType is { } retTy)
                    {
                        ret = Union.UnionType(ret, retTy);
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
        if (firstArg is LuaLiteralExprSyntax { Literal: LuaStringToken { InnerString: { } modulePath } })
        {
            if (context.Compilation.Workspace.Features.VirtualModule.TryGetValue(modulePath, out var realModule))
            {
                modulePath = realModule;
            }

            var document = context.Compilation.Workspace.FindModule(modulePath);
            if (document is not null)
            {
                context.Infer(document.SyntaxTree.SyntaxRoot);
            }
        }

        return context.Compilation.Builtin.Unknown;
    }
}
