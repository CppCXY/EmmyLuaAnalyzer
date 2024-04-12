using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Handlers;

public class UndefinedGlobalHandler(LuaCompilation compilation) : DiagnosticHandlerBase(compilation)
{
    public override List<DiagnosticCode> GetDiagnosticCodes() =>
        [DiagnosticCode.UndefinedGlobal, DiagnosticCode.NeedImport];

    public override void Check(DiagnosticContext context)
    {
        var semanticModel = Compilation.GetSemanticModel(context.Document.Id);
        if (semanticModel is null)
        {
            return;
        }

        var globals = context.Config.Globals;
        var nameExprs = semanticModel
            .Document
            .SyntaxTree
            .SyntaxRoot
            .Descendants
            .OfType<LuaNameExprSyntax>();

        var moduleToDocumentIds = Compilation.Workspace.ModuleGraph.ModuleNameToDocumentId;
        foreach (var nameExpr in nameExprs)
        {
            if (nameExpr is { Name: { RepresentText: { } name } nameToken } &&
                semanticModel.DeclarationTree.FindDeclaration(nameExpr, semanticModel.Context) is null)
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
                            if (Compilation.ProjectIndex.GetExportType(it) is { } ty
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
                            "undefined global",
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
                            $"import '{moduleIndex.ModulePath}'",
                            nameToken.Range,
                            Data: documentIds.First().Id.ToString()));
                    }
                    else
                    {
                        context.Report(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.NeedImport,
                            "need import from multiple modules",
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
                        "undefined global",
                        nameToken.Range));
                }
            }
        }
    }
}
