using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class UnusedChecker(LuaCompilation compilation) : DiagnosticCheckerBase(compilation, [DiagnosticCode.Unused])
{
    public override void Check(DiagnosticContext context)
    {
        var declarations = context.SearchContext.GetDocumentLocalDeclarations(context.Document.Id);

        foreach (var luaDeclaration in declarations)
        {
            if (luaDeclaration.IsLocal && context.SearchContext.FindReferences(luaDeclaration).Count() <= 1)
            {
                if (luaDeclaration.Name == "_")
                {
                    continue;
                }

                if (luaDeclaration.Info.Ptr.ToNode(context.SearchContext) is { } node)
                {
                    context.Report(
                        DiagnosticCode.Unused,
                        $"Unused variable {luaDeclaration.Name}",
                        node.Range,
                        DiagnosticTag.Unnecessary
                    );
                }
            }
        }
    }
}
