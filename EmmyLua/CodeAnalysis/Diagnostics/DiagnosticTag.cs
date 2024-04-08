namespace EmmyLua.CodeAnalysis.Diagnostics;

[Flags]
public enum DiagnosticTag
{
    None = 0x0,
    Unnecessary = 0x1,
    Deprecated = 0x2,
}
