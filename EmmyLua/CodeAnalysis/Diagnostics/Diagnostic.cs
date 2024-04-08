using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class Diagnostic(
    DiagnosticSeverity severity,
    DiagnosticCode code,
    string message,
    SourceRange range,
    DiagnosticTag tag = DiagnosticTag.None)
{
    public DiagnosticSeverity Severity { get; } = severity;

    public string Message { get; } = message;

    public SourceRange Range { get; } = range;

    public DiagnosticCode Code { get; } = code;

    public LuaLocation? Location { get; private set; }

    public DiagnosticTag Tag { get; } = tag;

    public Diagnostic(DiagnosticSeverity severity, DiagnosticCode code, string message, LuaLocation location,
        DiagnosticTag tag = DiagnosticTag.None)
        : this(severity, code, message, location.Range, tag)
    {
        Location = location;
    }

    public Diagnostic(DiagnosticSeverity severity, string message, LuaLocation location,
        DiagnosticTag tag = DiagnosticTag.None)
        : this(severity, DiagnosticCode.SyntaxError, message, location, tag)
    {
    }

    public Diagnostic(DiagnosticSeverity severity, string message, SourceRange range,
        DiagnosticTag tag = DiagnosticTag.None)
        : this(severity, DiagnosticCode.SyntaxError, message, range, tag)
    {
    }

    public Diagnostic WithLocation(LuaLocation location)
    {
        return new Diagnostic(Severity, Code, Message, location, Tag);
    }

    public override string ToString()
    {
        return Location != null
            ? $"{Location}: {Severity}: {Message} ({Code})"
            : $"{Range}: {Severity}: {Message} ({Code})";
    }
}
