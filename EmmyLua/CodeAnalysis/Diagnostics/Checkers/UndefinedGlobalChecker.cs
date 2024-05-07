using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class UndefinedGlobalChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation,
        [DiagnosticCode.UndefinedGlobal, DiagnosticCode.NeedImport])
{
    public override void Check(DiagnosticContext context)
    {
        var globals = context.Config.Globals;
        var nameExprs = context
            .Document
            .SyntaxTree
            .SyntaxRoot
            .Descendants
            .OfType<LuaNameExprSyntax>();

        var moduleToDocumentIds = Compilation.Workspace.ModuleGraph.ModuleNameToDocumentId;
        foreach (var nameExpr in nameExprs)
        {
            if (nameExpr is { Name: { RepresentText: { } name } nameToken } &&
                context.SearchContext.FindDeclaration(nameExpr) is null)
            {
                if (globals.Contains(name))
                {
                    continue;
                }

                if (moduleToDocumentIds.TryGetValue(name, out var documentIds))
                {
                    documentIds = documentIds
                        .Where(it =>
                        {
                            if (Compilation.DbManager.GetModuleExportType(it) is { } ty
                                && !ty.Equals(Builtin.Unknown) && !ty.Equals(Builtin.Nil))
                            {
                                return true;
                            }

                            return false;
                        }).ToList();

                    if (documentIds.Count == 0)
                    {
                        context.Report(new Diagnostic(
                            DiagnosticSeverity.Error,
                            DiagnosticCode.UndefinedGlobal,
                            "Undefined global",
                            nameToken.Range
                        ));
                        continue;
                    }

                    if (documentIds.Count == 1)
                    {
                        var moduleIndex = Compilation.Workspace.ModuleGraph.GetModuleInfo(documentIds.First());
                        if (moduleIndex is null)
                        {
                            continue;
                        }

                        context.Report(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.NeedImport,
                            $"Import '{moduleIndex.ModulePath}'",
                            nameToken.Range,
                            Data: documentIds.First().Id.ToString()));
                    }
                    else
                    {
                        context.Report(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.NeedImport,
                            "Need import from multiple modules",
                            nameToken.Range,
                            Data: string.Join(",", documentIds.Select(d => d.Id.ToString()))
                        ));
                    }
                }
                else
                {
                    context.Report(new Diagnostic(
                        DiagnosticSeverity.Error,
                        DiagnosticCode.UndefinedGlobal,
                        "Undefined global",
                        nameToken.Range));
                }
            }
        }
    }
}
