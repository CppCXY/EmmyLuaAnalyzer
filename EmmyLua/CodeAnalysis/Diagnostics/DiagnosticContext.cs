using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class DiagnosticContext(LuaDocument document)
{
    public LuaDocument Document { get; } = document;

    public void Report(Diagnostic diagnostic)
    {
        Document.SyntaxTree.PushDiagnostic(diagnostic);
    }
}
