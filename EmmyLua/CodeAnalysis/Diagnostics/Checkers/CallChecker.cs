using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
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

    private void CheckMissingParameter(DiagnosticContext context, LuaCallExprSyntax callExpr, LuaDeclaration luaDeclaration)
    {
        context.SearchContext.FindMethodsForType(luaDeclaration.Type, luaMethodType =>
        {
            var args = callExpr.ArgList?.ArgList.ToList() ?? [];
            var perfectSignature = context.SearchContext.FindPerfectMatchSignature(luaMethodType, callExpr, args);
            var parameters = perfectSignature.Parameters;
            var colonDefine = luaMethodType.ColonDefine;
            var colonCall = (callExpr.PrefixExpr as LuaIndexExprSyntax)?.IsColonIndex ?? false;

            switch ((colonDefine, colonCall))
            {
                case (true, false):
                {
                    var oldParameters = parameters;
                    parameters = [new Compilation.Declaration.LuaDeclaration("self", new VirtualInfo(Builtin.Unknown))];
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
        });
    }

    private void CheckNoDiscard(DiagnosticContext context, LuaCallExprSyntax callExpr, LuaDeclaration luaDeclaration)
    {
        if (callExpr.Parent is LuaCallStatSyntax && luaDeclaration.IsNoDiscard)
        {
            context.Report(
                DiagnosticCode.NoDiscard,
                $"No discard for function '{luaDeclaration.Name}'",
                callExpr.Range
            );
        }
    }
}
