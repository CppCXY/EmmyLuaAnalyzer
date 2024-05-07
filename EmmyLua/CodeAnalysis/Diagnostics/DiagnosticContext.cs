using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class DiagnosticContext(LuaDocument document, LuaDiagnostics luaDiagnostics, SearchContext searchContext)
{
    private LuaDiagnostics LuaDiagnostics { get; } = luaDiagnostics;

    public LuaDocument Document { get; } = document;

    public DiagnosticConfig Config => LuaDiagnostics.Config;

    public SearchContext SearchContext { get; } = searchContext;

    public void Report(Diagnostic diagnostic)
    {
        if (LuaDiagnostics.CanAddDiagnostic(Document.Id, diagnostic.Code, diagnostic.Range))
        {
            LuaDiagnostics.AddDiagnostic(Document.Id, diagnostic);
        }
    }
}
