using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Handlers;

public abstract class DiagnosticHandlerBase(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public virtual void Check(DiagnosticContext context)
    {
    }
}
