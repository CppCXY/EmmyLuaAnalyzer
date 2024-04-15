using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Diagnostics.Handlers;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class LuaDiagnostics(LuaCompilation compilation)
{
    private List<DiagnosticHandlerBase> Handlers { get; } = new()
    {
        new UnusedHandler(compilation),
        new UndefinedGlobalHandler(compilation),
        new TypeCheckHandler(compilation)
    };

    public LuaCompilation Compilation { get; } = compilation;

    private HashSet<LuaDocumentId> IsMetaDocument { get; } = new();

    private Dictionary<LuaDocumentId, HashSet<DiagnosticCode>> Disables { get; } = new();

    private Dictionary<LuaDocumentId, HashSet<DiagnosticCode>> Enables { get; } = new();

    private Dictionary<LuaDocumentId, DisableNextLine> DisableNextLines { get; } = new();

    public DiagnosticConfig Config { get; set; } = new();

    public void Check(LuaDocument document)
    {
        if (IsMetaDocument.Contains(document.Id))
        {
            return;
        }

        var context = new DiagnosticContext(document, this);
        foreach (var handler in Handlers)
        {
            if (CanCheck(document.Id, handler))
            {
                handler.Check(context);
            }
        }
    }

    public bool CanCheck(LuaDocumentId documentId, DiagnosticHandlerBase handlerBase)
    {
        var codes = handlerBase.Codes;
        return codes.Count != 0 && codes.Any(code => CanCheckCode(documentId, code));
    }

    private bool CanCheckCode(LuaDocumentId documentId, DiagnosticCode code)
    {
        var shouldCheck = !Config.WorkspaceDisabledCodes.Contains(code);
        if (Disables.TryGetValue(documentId, out var disables))
        {
            if (disables.Contains(code))
            {
                shouldCheck = false;
            }
        }

        if (Enables.TryGetValue(documentId, out var enables))
        {
            if (enables.Contains(code))
            {
                shouldCheck = true;
            }
        }

        return shouldCheck;
    }

    public bool CanAddDiagnostic(LuaDocumentId documentId, DiagnosticCode code, SourceRange range)
    {
        if (!CanCheckCode(documentId, code))
        {
            return false;
        }

        if (DisableNextLines.TryGetValue(documentId, out var disableNextLine))
        {
            if (disableNextLine.Ranges.TryGetValue(code, out var ranges))
            {
                return ranges.All(disableRange => !disableRange.Intersect(range));
            }
        }

        return true;
    }

    public void AddMeta(LuaDocumentId documentId)
    {
        IsMetaDocument.Add(documentId);
    }

    public void AddDiagnosticDisable(LuaDocumentId documentId, string diagnosticName)
    {
        var code = DiagnosticCodeHelper.GetCode(diagnosticName);
        if (code != DiagnosticCode.None)
        {
            if (!Disables.TryGetValue(documentId, out var disables))
            {
                disables = new HashSet<DiagnosticCode>();
                Disables[documentId] = disables;
            }

            disables.Add(code);
        }
    }

    public void AddDiagnosticEnable(LuaDocumentId documentId, string diagnosticName)
    {
        var code = DiagnosticCodeHelper.GetCode(diagnosticName);
        if (code != DiagnosticCode.None)
        {
            if (!Enables.TryGetValue(documentId, out var enables))
            {
                enables = new HashSet<DiagnosticCode>();
                Enables[documentId] = enables;
            }

            enables.Add(code);
        }
    }

    public void AddDiagnosticDisableNextLine(LuaDocumentId documentId, SourceRange range, string diagnosticName)
    {
        var code = DiagnosticCodeHelper.GetCode(diagnosticName);
        if (code != DiagnosticCode.None)
        {
            if (!DisableNextLines.TryGetValue(documentId, out var disableNextLine))
            {
                disableNextLine = new DisableNextLine();
                DisableNextLines[documentId] = disableNextLine;
            }

            if (!disableNextLine.Ranges.TryGetValue(code, out var ranges))
            {
                ranges = new List<SourceRange>();
                disableNextLine.Ranges[code] = ranges;
            }

            ranges.Add(range);
        }
    }

    public void RemoveCache(LuaDocumentId documentId)
    {
        IsMetaDocument.Remove(documentId);
        Disables.Remove(documentId);
        Enables.Remove(documentId);
        DisableNextLines.Remove(documentId);
    }

    public List<string> GetDiagnosticNames()
    {
        return Handlers.SelectMany(handler => handler.Codes.Select(DiagnosticCodeHelper.GetName)).ToList();
    }
}
