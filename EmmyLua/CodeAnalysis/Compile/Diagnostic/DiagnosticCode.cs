namespace EmmyLua.CodeAnalysis.Compile.Diagnostic;

public enum DiagnosticCode
{
    SyntaxError,

    TypeNotFound,
    MissingReturn,
    TypeNotMatch,
    MissingParameter,
    InjectFieldFail,
    UnreachableCode,
}
