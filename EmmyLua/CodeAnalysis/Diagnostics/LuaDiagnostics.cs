using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Diagnostics.Handlers;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Diagnostics;

public class LuaDiagnostics
{
    private List<DiagnosticHandlerBase> Handlers { get; }

    private Dictionary<DiagnosticCode, DiagnosticHandlerBase> HandlerMap { get; }

    public LuaCompilation Compilation { get; }

    public LuaDiagnostics(LuaCompilation compilation)
    {
        Compilation = compilation;
        Handlers = new List<DiagnosticHandlerBase>
        {
            new UnusedHandler(compilation)
        };
        HandlerMap = new Dictionary<DiagnosticCode, DiagnosticHandlerBase>();
        foreach (var handler in Handlers)
        {
            foreach (var code in handler.GetDiagnosticCodes())
            {
                HandlerMap.TryAdd(code, handler);
            }
        }
    }

    public void Check(LuaDocument document)
    {
        var context = new DiagnosticContext(document);
        foreach (var handler in Handlers)
        {
            handler.Check(context);
        }
    }
}
