using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public record Diagnostic(
    DiagnosticSeverity Severity,
    DiagnosticCode Code,
    string Message,
    SourceRange Range,
    DiagnosticTag Tag = DiagnosticTag.None,
    string? Data = null
    )
{
    public override string ToString()
    {
        return $"{Range}: {Severity}: {Message} ({Code})";
    }


}
