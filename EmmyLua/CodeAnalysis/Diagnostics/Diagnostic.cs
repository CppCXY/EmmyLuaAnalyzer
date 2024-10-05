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
    public static Diagnostic Error(DiagnosticCode code, string message, SourceRange range, string? data = null)
    {
        return new Diagnostic(DiagnosticSeverity.Error, code, message, range, DiagnosticTag.None, data);
    }

    public static Diagnostic Warning(DiagnosticCode code, string message, SourceRange range, string? data = null)
    {
        return new Diagnostic(DiagnosticSeverity.Warning, code, message, range, DiagnosticTag.None, data);
    }

    public static Diagnostic Info(DiagnosticCode code, string message, SourceRange range, string? data = null)
    {
        return new Diagnostic(DiagnosticSeverity.Information, code, message, range, DiagnosticTag.None, data);
    }

    public override string ToString()
    {
        return $"{Range}: {Severity}: {Message} ({Code})";
    }
}
