using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Diagnostic;

public class Diagnostic
{
    public DiagnosticSeverity Severity { get; }

    public string Message { get; }

    public SourceRange Range { get; }

    public LuaLocation? Location { get; private set; }

    public Diagnostic(DiagnosticSeverity severity, string message, SourceRange range)
    {
        Severity = severity;
        Message = message;
        Range = range;
    }

    public Diagnostic(DiagnosticSeverity severity, string message, LuaLocation location)
    {
        Severity = severity;
        Message = message;
        Location = location;
        Range = location.Range;
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
