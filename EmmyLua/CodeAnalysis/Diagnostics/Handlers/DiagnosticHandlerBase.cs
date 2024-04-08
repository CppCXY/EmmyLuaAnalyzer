using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Handlers;

public abstract class DiagnosticHandlerBase(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public virtual List<DiagnosticCode> GetDiagnosticCodes() => new List<DiagnosticCode>();

    public virtual void Check(DiagnosticContext context)
    {
    }

    public virtual void Fix(Diagnostic diagnostic)
    {
    }
}
