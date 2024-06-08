using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
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

    private void CheckMissingParameter(DiagnosticContext context, LuaCallExprSyntax callExpr, IDeclaration declaration)
    {
        context.SearchContext.FindMethodsForType(declaration.Type, luaMethodType =>
        {
            var args = callExpr.ArgList?.ArgList.ToList() ?? [];
            var perfectSignature = luaMethodType.FindPerfectMatchSignature(callExpr, args, context.SearchContext);
            var parameters = perfectSignature.Parameters;
            var lastToken = callExpr.ArgList?.LastToken();
            if (parameters.Count > args.Count && lastToken is not null)
            {
                for (int i = args.Count; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter is LuaDeclaration { Info: ParamInfo paramInfo })
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

    private void CheckNoDiscard(DiagnosticContext context, LuaCallExprSyntax callExpr, IDeclaration declaration)
    {
        if (callExpr.Parent is LuaCallStatSyntax && declaration.IsNoDiscard)
        {
            context.Report(
                DiagnosticCode.NoDiscard,
                $"No discard for function '{declaration.Name}'",
                callExpr.Range
            );
        }
    }
}
