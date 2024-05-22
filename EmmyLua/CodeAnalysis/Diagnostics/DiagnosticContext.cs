using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class DiagnosticContext(
    LuaDocument document,
    LuaDiagnostics luaDiagnostics,
    SearchContext searchContext,
    List<Diagnostic> diagnostics)
{
    private LuaDiagnostics LuaDiagnostics { get; } = luaDiagnostics;

    public LuaDocument Document { get; } = document;

    public DiagnosticConfig Config => LuaDiagnostics.Config;

    public SearchContext SearchContext { get; } = searchContext;

    public void Report(
        DiagnosticCode code,
        string message,
        SourceRange range,
        DiagnosticTag tag = DiagnosticTag.None,
        string? data = null)
    {
        if (LuaDiagnostics.CanAddDiagnostic(Document.Id, code, range))
        {
            var severity = Config.SeverityOverrides.TryGetValue(code, out var severityOverride)
                ? severityOverride
                : DiagnosticSeverityHelper.GetDefaultSeverity(code);

            var diagnostic = new Diagnostic(severity, code, message, range, tag, data);
            diagnostics.Add(diagnostic);
        }
    }
}
