using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Diagnostic;

public class Diagnostic(DiagnosticSeverity severity, string message, SourceRange range)
{
    public DiagnosticSeverity Severity { get; } = severity;

    public string Message { get; } = message;

    public SourceRange Range { get; } = range;

    public LuaLocation? Location { get; private set; }

    public Diagnostic(DiagnosticSeverity severity, string message, LuaLocation location) : this(severity, message, location.Range)
    {
        Location = location;
    }

    public Diagnostic WithLocation(LuaLocation location)
    {
        return new Diagnostic(Severity, Message, location);
    }

    public override string ToString()
    {
        return Location != null ? $"{Location}: {Severity}: {Message}" : $"{Range}: {Severity}: {Message}";
    }
}
