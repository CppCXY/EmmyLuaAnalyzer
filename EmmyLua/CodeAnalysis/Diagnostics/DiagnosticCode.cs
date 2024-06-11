using System.Runtime.Serialization;

namespace EmmyLua.CodeAnalysis.Diagnostics;

// @formatter:off
public enum DiagnosticCode
{
    [EnumMember(Value = "none")]
    None,
    [EnumMember(Value = "syntax-error")]
    SyntaxError,
    [EnumMember(Value = "type-not-found")]
    TypeNotFound,
    [EnumMember(Value = "missing-return")]
    MissingReturn,
    [EnumMember(Value = "type-not-match")]
    TypeNotMatch,
    [EnumMember(Value = "missing-parameter")]
    MissingParameter,
    [EnumMember(Value = "inject-field-fail")]
    InjectFieldFail,
    [EnumMember(Value = "unreachable-code")]
    UnreachableCode,
    [EnumMember(Value = "unused")]
    Unused,
    [EnumMember(Value = "undefined-global")]
    UndefinedGlobal,
    [EnumMember(Value = "need-import")]
    NeedImport,
    [EnumMember(Value = "deprecated")]
    Deprecated,
    [EnumMember(Value = "access-private-member")]
    AccessPrivateMember,
    [EnumMember(Value = "access-protected-member")]
    AccessProtectedMember,
    [EnumMember(Value = "access-package-member")]
    AccessPackageMember,
    [EnumMember(Value = "no-discard")]
    NoDiscard,
    [EnumMember(Value = "disable-global-define")]
    DisableGlobalDefine,
    [EnumMember(Value = "undefined-field")]
    UndefinedField,
    [EnumMember(Value = "local-const-reassign")]
    LocalConstReassign,

}
// @formatter:on
public static class DiagnosticCodeHelper
{
    private static readonly Dictionary<DiagnosticCode, string> NameCache = new();
    private static readonly Dictionary<string, DiagnosticCode> CodeCache = new();

    static DiagnosticCodeHelper()
    {
        var type = typeof(DiagnosticCode);
        foreach (DiagnosticCode code in Enum.GetValues(type))
        {
            var field = type.GetField(code.ToString());
            var attr =
                field?.GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault() as EnumMemberAttribute;
            if (attr?.Value == null) continue;
            NameCache[code] = attr.Value;
            CodeCache[attr.Value] = code;
        }
    }

    public static string GetName(DiagnosticCode code)
    {
        return NameCache.TryGetValue(code, out var name) ? name : code.ToString();
    }

    public static DiagnosticCode GetCode(string name)
    {
        return CodeCache.GetValueOrDefault(name, DiagnosticCode.None);
    }

    public static bool IsCodeDefaultEnable(DiagnosticCode code)
    {
        return code switch
        {
            DiagnosticCode.SyntaxError => true,
            DiagnosticCode.TypeNotFound => false,
            DiagnosticCode.MissingReturn => true,
            DiagnosticCode.TypeNotMatch => true,
            DiagnosticCode.MissingParameter => true,
            DiagnosticCode.InjectFieldFail => true,
            DiagnosticCode.UnreachableCode => true,
            DiagnosticCode.Unused => true,
            DiagnosticCode.UndefinedGlobal => true,
            DiagnosticCode.NeedImport => true,
            DiagnosticCode.Deprecated => true,
            DiagnosticCode.AccessPrivateMember => true,
            DiagnosticCode.AccessPackageMember => true,
            DiagnosticCode.AccessProtectedMember => true,
            DiagnosticCode.NoDiscard => true,
            DiagnosticCode.DisableGlobalDefine => false,
            DiagnosticCode.UndefinedField => true,
            DiagnosticCode.LocalConstReassign => true,
            _ => false
        };
    }
}
