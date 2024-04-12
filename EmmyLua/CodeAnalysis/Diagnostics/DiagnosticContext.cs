using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class DiagnosticContext(LuaDocument document, DiagnosticConfig config)
{
    public LuaDocument Document { get; } = document;

    public DiagnosticConfig Config { get; } = config;

    public void Report(Diagnostic diagnostic)
    {
        Document.SyntaxTree.PushDiagnostic(diagnostic);
    }
}
