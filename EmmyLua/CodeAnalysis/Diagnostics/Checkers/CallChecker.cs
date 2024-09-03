using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;


namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class CallChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.MissingParameter,
        DiagnosticCode.NoDiscard
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var document = context.Document;
        foreach (var callExpr in document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaCallExprSyntax>())
        {
            var declaration = context.SearchContext.FindDeclaration(callExpr);
            if (declaration is null)
            {
                continue;
            }

            CheckMissingParameter(context, callExpr, declaration);
            CheckNoDiscard(context, callExpr, declaration);
        }
    }

    private void CheckMissingParameter(DiagnosticContext context, LuaCallExprSyntax callExpr, LuaSymbol luaSymbol)
    {
        foreach (var luaMethodType in context.SearchContext.FindCallableType(luaSymbol.Type))
        {
            var args = callExpr.ArgList?.ArgList.ToList() ?? [];
            var perfectSignature = context.SearchContext.FindPerfectMatchSignature(luaMethodType, callExpr, args);
            var parameters = perfectSignature.Parameters;
            var colonDefine = perfectSignature.ColonDefine;
            var colonCall = (callExpr.PrefixExpr as LuaIndexExprSyntax)?.IsColonIndex ?? false;

            switch ((colonDefine, colonCall))
            {
                case (true, false):
                {
                    var oldParameters = parameters;
                    parameters = [new LuaSymbol("self", Builtin.Unknown, new VirtualInfo())];
                    parameters.AddRange(oldParameters);
                    break;
                }
                case (false, true):
                {
                    parameters = parameters.Skip(1).ToList();
                    break;
                }
            }

            var lastToken = callExpr.ArgList?.LastToken();
            if (lastToken is null)
            {
                return;
            }

            if (parameters.Count > args.Count)
            {
                for (var i = args.Count; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter is { Info: ParamInfo paramInfo })
                    {
                        if (paramInfo.IsVararg || paramInfo.Nullable)
                        {
                            continue;
                        }
                    }

                    context.Report(
                        DiagnosticCode.MissingParameter,
                        $"Missing parameter '{parameter.Name}'",
                        lastToken.Range
                    );
                }
            }
        }
    }

    private void CheckNoDiscard(DiagnosticContext context, LuaCallExprSyntax callExpr, LuaSymbol luaSymbol)
    {
        if (callExpr.Parent is LuaCallStatSyntax && luaSymbol.IsNoDiscard)
        {
            context.Report(
                DiagnosticCode.NoDiscard,
                $"No discard for function '{luaSymbol.Name}'",
                callExpr.Range
            );
        }
    }
}
