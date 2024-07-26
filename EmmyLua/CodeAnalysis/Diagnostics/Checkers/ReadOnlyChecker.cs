using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class ReadOnlyChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.LocalConstReassign
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var document = context.Document;
        var declarations = context.SearchContext.GetDocumentLocalDeclarations(document.Id);
        foreach (var declaration in declarations)
        {
            if (declaration is { IsLocal: true, Info: LocalInfo { IsConst: true } })
            {
                var references = context.SearchContext.FindReferences(declaration);
                foreach (var reference in references)
                {
                    if (reference.Kind == ReferenceKind.Write)
                    {
                        context.Report(
                            DiagnosticCode.LocalConstReassign,
                            $"Cannot reassign to const variable {declaration.Name}",
                            reference.Element.Range
                        );
                    }
                }
            }
        }
    }
}
