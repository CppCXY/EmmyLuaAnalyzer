using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EmmyLua.CodeAnalysis.Diagnostics;

[JsonConverter(typeof(JsonStringEnumConverter<DiagnosticSeverity>))]
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
            DiagnosticCode.TypeNotFound => DiagnosticSeverity.Warning,
            DiagnosticCode.MissingReturn => DiagnosticSeverity.Warning,
            DiagnosticCode.TypeNotMatch => DiagnosticSeverity.Warning,
            DiagnosticCode.MissingParameter => DiagnosticSeverity.Warning,
            DiagnosticCode.InjectFieldFail => DiagnosticSeverity.Error,
            DiagnosticCode.UnreachableCode => DiagnosticSeverity.Hint,
            DiagnosticCode.Unused => DiagnosticSeverity.Hint,
            DiagnosticCode.UndefinedGlobal => DiagnosticSeverity.Error,
            DiagnosticCode.NeedImport => DiagnosticSeverity.Warning,
            DiagnosticCode.Deprecated => DiagnosticSeverity.Hint,
            DiagnosticCode.AccessPrivateMember => DiagnosticSeverity.Warning,
            DiagnosticCode.AccessPackageMember => DiagnosticSeverity.Warning,
            DiagnosticCode.AccessProtectedMember => DiagnosticSeverity.Warning,
            DiagnosticCode.NoDiscard => DiagnosticSeverity.Warning,
            DiagnosticCode.DisableGlobalDefine => DiagnosticSeverity.Error,
            DiagnosticCode.UndefinedField => DiagnosticSeverity.Warning,
            DiagnosticCode.LocalConstReassign => DiagnosticSeverity.Error,
            _ => DiagnosticSeverity.Warning
        };
    }
}
