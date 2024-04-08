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
    Unused
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
            _ => DiagnosticCode.None
        };
    }
}
