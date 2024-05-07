using System.Runtime.Serialization;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public enum DiagnosticSeverity
{
    [EnumMember(Value = "error")]
    Error = 0,
    [EnumMember(Value = "warning")]
    Warning = 1,
    [EnumMember(Value = "information")]
    Information = 2,
    [EnumMember(Value = "hint")]
    Hint = 3,
}

public static class DiagnosticSeverityHelper
{
    private static readonly Dictionary<DiagnosticSeverity, string> NameCache = new();
    private static readonly Dictionary<string, DiagnosticSeverity> SeverityCache = new();

    static DiagnosticSeverityHelper()
    {
        var type = typeof(DiagnosticSeverity);
        foreach (DiagnosticSeverity severity in Enum.GetValues(type))
        {
            var field = type.GetField(severity.ToString());
            var attr =
                field?.GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault() as EnumMemberAttribute;
            if (attr?.Value == null) continue;
            NameCache[severity] = attr.Value;
            SeverityCache[attr.Value] = severity;
        }
    }

    public static string GetName(DiagnosticSeverity severity)
    {
        return NameCache.TryGetValue(severity, out var name) ? name : severity.ToString();
    }

    public static DiagnosticSeverity GetSeverity(string name)
    {
        return SeverityCache.GetValueOrDefault(name, DiagnosticSeverity.Error);
    }

    public static DiagnosticSeverity GetDefaultSeverity(DiagnosticCode code)
    {
        return code switch
        {
            DiagnosticCode.SyntaxError => DiagnosticSeverity.Error,
            DiagnosticCode.TypeNotFound => DiagnosticSeverity.Error,
            DiagnosticCode.MissingReturn => DiagnosticSeverity.Error,
            DiagnosticCode.TypeNotMatch => DiagnosticSeverity.Error,
            DiagnosticCode.MissingParameter => DiagnosticSeverity.Error,
            DiagnosticCode.InjectFieldFail => DiagnosticSeverity.Error,
            DiagnosticCode.UnreachableCode => DiagnosticSeverity.Error,
            DiagnosticCode.Unused => DiagnosticSeverity.Hint,
            DiagnosticCode.UndefinedGlobal => DiagnosticSeverity.Error,
            DiagnosticCode.NeedImport => DiagnosticSeverity.Warning,
            DiagnosticCode.Deprecated => DiagnosticSeverity.Hint,
            DiagnosticCode.AccessPrivateMember => DiagnosticSeverity.Error,
            _ => DiagnosticSeverity.Error
        };
    }
}
