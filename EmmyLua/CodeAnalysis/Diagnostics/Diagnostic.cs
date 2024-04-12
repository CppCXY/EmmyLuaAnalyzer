using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public record Diagnostic(
    DiagnosticSeverity Severity,
    DiagnosticCode Code,
    string Message,
    SourceRange Range,
    DiagnosticTag Tag = DiagnosticTag.None,
    LuaLocation? Location = null,
    string? Data = null
    )
{
    public override string ToString()
    {
        return Location != null
            ? $"{Location}: {Severity}: {Message} ({Code})"
            : $"{Range}: {Severity}: {Message} ({Code})";
    }
}
