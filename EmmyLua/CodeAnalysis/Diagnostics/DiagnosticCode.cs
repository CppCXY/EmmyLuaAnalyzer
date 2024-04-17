using System.Runtime.Serialization;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public enum DiagnosticCode
{
    [EnumMember(Value = "none")] None,
    [EnumMember(Value = "syntax-error")] SyntaxError,
    [EnumMember(Value = "type-not-found")] TypeNotFound,
    [EnumMember(Value = "missing-return")] MissingReturn,
    [EnumMember(Value = "type-not-match")] TypeNotMatch,

    [EnumMember(Value = "missing-parameter")]
    MissingParameter,

    [EnumMember(Value = "inject-field-fail")]
    InjectFieldFail,

    [EnumMember(Value = "unreachable-code")]
    UnreachableCode,
    [EnumMember(Value = "unused")] Unused,

    [EnumMember(Value = "undefined-global")]
    UndefinedGlobal,
    [EnumMember(Value = "need-import")] NeedImport,
}

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
}
