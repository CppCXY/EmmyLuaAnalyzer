using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Handlers;

public abstract class DiagnosticHandlerBase(LuaCompilation compilation, List<DiagnosticCode> codes)
{
    public LuaCompilation Compilation { get; } = compilation;

    public List<DiagnosticCode> Codes { get; } = codes;

    public virtual void Check(DiagnosticContext context)
    {
    }
}
