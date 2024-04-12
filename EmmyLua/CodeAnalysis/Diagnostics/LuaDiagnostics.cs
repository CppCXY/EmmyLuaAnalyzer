using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Diagnostics.Handlers;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class LuaDiagnostics
{
    private List<DiagnosticHandlerBase> Handlers { get; }

    private Dictionary<DiagnosticCode, DiagnosticHandlerBase> HandlerMap { get; }

    public LuaCompilation Compilation { get; }

    private HashSet<LuaDocumentId> IsMetaDocument { get; } = new();

    public DiagnosticConfig Config { get; set; } = new();

    public LuaDiagnostics(LuaCompilation compilation)
    {
        Compilation = compilation;
        Handlers = new List<DiagnosticHandlerBase>
        {
            new UnusedHandler(compilation),
            new UndefinedGlobalHandler(compilation)
        };
        HandlerMap = new Dictionary<DiagnosticCode, DiagnosticHandlerBase>();
        foreach (var handler in Handlers)
        {
            foreach (var code in handler.GetDiagnosticCodes())
            {
                HandlerMap.TryAdd(code, handler);
            }
        }
    }

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
