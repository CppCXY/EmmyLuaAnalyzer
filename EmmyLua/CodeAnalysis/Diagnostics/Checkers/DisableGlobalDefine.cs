using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class DisableGlobalDefine(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.DisableGlobalDefine
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var document = context.Document;
        var declarations = context.SearchContext.GetDocumentLocalDeclarations(document.Id);
        foreach (var declaration in declarations)
        {
            if (declaration.IsGlobal && declaration.Info.Ptr.ToNode(document) is {} node)
            {
                context.Report(
                    DiagnosticCode.DisableGlobalDefine,
                    $"Defining global variable {declaration.Name} is not allowed.",
                    node.Range
                );
            }
        }
    }
}
