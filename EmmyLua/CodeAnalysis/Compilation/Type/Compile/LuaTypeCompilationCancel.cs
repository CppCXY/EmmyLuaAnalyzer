using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public class LuaTypeCompilationCancel(DiagnosticCode code, string message, SourceRange range) : Exception(message)
{
    public DiagnosticCode Code { get; } = code;

    public SourceRange Range { get; } = range;
}
