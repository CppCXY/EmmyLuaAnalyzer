using EmmyLua.CodeAnalysis.Compile.Source;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compile.Diagnostic;

public class Diagnostic(DiagnosticSeverity severity, DiagnosticCode code, string message, SourceRange range)
{
    public DiagnosticSeverity Severity { get; } = severity;

    public string Message { get; } = message;

    public SourceRange Range { get; } = range;

    public DiagnosticCode Code { get; } = code;

    public LuaLocation? Location { get; private set; }

    public Diagnostic(DiagnosticSeverity severity, DiagnosticCode code, string message, LuaLocation location)
        : this(severity, code, message, location.Range)
    {
        Location = location;
    }

    public Diagnostic(DiagnosticSeverity severity, string message, LuaLocation location)
        : this(severity, DiagnosticCode.SyntaxError, message, location.Range)
    {
        Location = location;
    }

    public Diagnostic(DiagnosticSeverity severity, string message, SourceRange range)
        : this(severity, DiagnosticCode.SyntaxError, message, range)
    {
    }

    public Diagnostic WithLocation(LuaLocation location)
    {
        return new Diagnostic(Severity, Code, Message, location);
    }

    public override string ToString()
    {
        return Location != null
            ? $"{Location}: {Severity}: {Message} ({Code})"
            : $"{Range}: {Severity}: {Message} ({Code})";
    }
}
