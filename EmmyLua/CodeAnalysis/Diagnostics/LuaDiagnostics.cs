using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Diagnostics.Checkers;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class LuaDiagnostics(LuaCompilation compilation)
{
    private List<DiagnosticCheckerBase> Checkers { get; } =
    [
        new UnusedChecker(compilation),
        new UndefinedGlobalChecker(compilation),
        new TypeChecker(compilation),
        new DeprecatedChecker(compilation),
        new VisibilityChecker(compilation)
    ];

    public LuaCompilation Compilation { get; } = compilation;

    private HashSet<LuaDocumentId> IsMetaDocument { get; } = new();

    private Dictionary<LuaDocumentId, HashSet<DiagnosticCode>> Disables { get; } = new();

    private Dictionary<LuaDocumentId, HashSet<DiagnosticCode>> Enables { get; } = new();

    private Dictionary<LuaDocumentId, DisableNextLine> DisableNextLines { get; } = new();

    public DiagnosticConfig Config => Compilation.Workspace.Features.DiagnosticConfig;

    private Dictionary<LuaDocumentId, List<Diagnostic>> BasicSyntaxErrors { get; } = new();

    public bool Check(LuaDocument document, SearchContext searchContext, out List<Diagnostic> results)
    {
        results = new List<Diagnostic>();
        if (IsMetaDocument.Contains(document.Id))
        {
            return false;
        }

        results.AddRange(GetDiagnostics(document.Id));
        var context = new DiagnosticContext(document, this, searchContext, results);
        foreach (var checker in Checkers)
        {
            if (CanCheck(document.Id, checker))
            {
                checker.Check(context);
            }
        }

        return true;
    }

    public bool CanCheck(LuaDocumentId documentId, DiagnosticCheckerBase checkerBase)
    {
        var codes = checkerBase.Codes;
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

    public void AddDiagnostic(LuaDocumentId documentId, Diagnostic diagnostic)
    {
        if (!BasicSyntaxErrors.TryGetValue(documentId, out var diagnostics))
        {
            diagnostics = new List<Diagnostic>();
            BasicSyntaxErrors[documentId] = diagnostics;
        }

        diagnostics.Add(diagnostic);
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
        BasicSyntaxErrors.Remove(documentId);
    }

    public void ClearDiagnostic(LuaDocumentId documentId)
    {
        BasicSyntaxErrors.Remove(documentId);
    }

    public void ClearAllDiagnostic()
    {
        BasicSyntaxErrors.Clear();
    }

    public IEnumerable<Diagnostic> GetDiagnostics(LuaDocumentId documentId)
    {
        return BasicSyntaxErrors.GetValueOrDefault(documentId) ?? Enumerable.Empty<Diagnostic>();
    }

    public List<string> GetDiagnosticNames()
    {
        return Checkers.SelectMany(handler => handler.Codes.Select(DiagnosticCodeHelper.GetName)).ToList();
    }

    public void AddChecker(DiagnosticCheckerBase checker)
    {
        Checkers.Add(checker);
    }

    /// <summary>
    /// for custom diagnostic
    /// </summary>
    public void ClearCheckers()
    {
        Checkers.Clear();
    }
}
