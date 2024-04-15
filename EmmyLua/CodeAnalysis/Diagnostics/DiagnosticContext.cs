using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class DiagnosticContext(LuaDocument document, LuaDiagnostics luaDiagnostics)
{
    private LuaDiagnostics LuaDiagnostics { get; } = luaDiagnostics;

    public LuaDocument Document { get; } = document;

    public DiagnosticConfig Config => LuaDiagnostics.Config;

    public void Report(Diagnostic diagnostic)
    {
        if (luaDiagnostics.CanAddDiagnostic(Document.Id, diagnostic.Code, diagnostic.Range))
        {
            Document.SyntaxTree.PushDiagnostic(diagnostic);
        }
    }
}
