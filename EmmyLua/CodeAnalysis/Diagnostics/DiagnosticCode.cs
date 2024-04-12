namespace EmmyLua.CodeAnalysis.Diagnostics;

public enum DiagnosticCode
{
    None,
    SyntaxError,

    TypeNotFound,
    MissingReturn,
    TypeNotMatch,
    MissingParameter,
    InjectFieldFail,
    UnreachableCode,
    Unused,
    UndefinedGlobal,
    NeedImport,
}

public static class DiagnosticCodeHelper
{
    public static string GetName(DiagnosticCode code)
    {
        return code switch
        {
            DiagnosticCode.SyntaxError => "syntax-error",
            DiagnosticCode.TypeNotFound => "type-not-found",
            DiagnosticCode.MissingReturn => "missing-return",
            DiagnosticCode.TypeNotMatch => "type-not-match",
            DiagnosticCode.MissingParameter => "missing-parameter",
            DiagnosticCode.InjectFieldFail => "inject-field-fail",
            DiagnosticCode.UnreachableCode => "unreachable-code",
            DiagnosticCode.Unused => "unused",
            DiagnosticCode.UndefinedGlobal => "undefined-global",
            DiagnosticCode.NeedImport => "need-import",
            _ => "none"
        };
    }

    public static DiagnosticCode GetCode(string name)
    {
        return name switch
        {
            "syntax-error" => DiagnosticCode.SyntaxError,
            "type-not-found" => DiagnosticCode.TypeNotFound,
            "missing-return" => DiagnosticCode.MissingReturn,
            "type-not-match" => DiagnosticCode.TypeNotMatch,
            "missing-parameter" => DiagnosticCode.MissingParameter,
            "inject-field-fail" => DiagnosticCode.InjectFieldFail,
            "unreachable-code" => DiagnosticCode.UnreachableCode,
            "unused" => DiagnosticCode.Unused,
            "undefined-global" => DiagnosticCode.UndefinedGlobal,
            "need-import" => DiagnosticCode.NeedImport,
            _ => DiagnosticCode.None
        };
    }
}
