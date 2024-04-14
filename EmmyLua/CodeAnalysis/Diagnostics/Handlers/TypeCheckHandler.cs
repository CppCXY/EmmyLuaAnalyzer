using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Handlers;

public class TypeCheckHandler(LuaCompilation compilation) : DiagnosticHandlerBase(compilation)
{
    public override void Check(DiagnosticContext context)
    {
        foreach (var node in context.Document.SyntaxTree.SyntaxRoot.Descendants)
        {
            // TODO
        }
    }
}
