using System.Text.RegularExpressions;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class DiagnosticConfig
{
    public HashSet<string> Globals { get; } = new();

    public List<Regex> GlobalRegexes { get; } = new();

    public HashSet<DiagnosticCode> WorkspaceDisabledCodes { get; } = new();

    public Dictionary<DiagnosticCode, DiagnosticSeverity> SeverityOverrides { get; } = new();
}

public class DisableNextLine
{
    public Dictionary<DiagnosticCode, List<SourceRange>> Ranges { get; } = new();
}
