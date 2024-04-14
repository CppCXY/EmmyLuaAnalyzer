using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Diagnostics.Handlers;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class LuaDiagnostics(LuaCompilation compilation)
{
    private List<DiagnosticHandlerBase> Handlers { get; } = new()
    {
        new UnusedHandler(compilation),
        new UndefinedGlobalHandler(compilation)
    };

    public LuaCompilation Compilation { get; } = compilation;

    private HashSet<LuaDocumentId> IsMetaDocument { get; } = new();

    public DiagnosticConfig Config { get; set; } = new();

    public void Check(LuaDocument document)
    {
        if (IsMetaDocument.Contains(document.Id))
        {
            return;
        }

        var context = new DiagnosticContext(document, Config);
        foreach (var handler in Handlers)
        {
            handler.Check(context);
        }
    }

    public void AddMeta(LuaDocumentId documentId)
    {
        IsMetaDocument.Add(documentId);
    }

    public void RemoveCache(LuaDocumentId documentId)
    {
        IsMetaDocument.Remove(documentId);
    }
}
